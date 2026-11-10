using AutoMapper;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Lead (ANALISI_CRM.md §5.2): contatti non ancora qualificati. Con la conversione diventano un cliente (nuovo, con i
     dati del lead, o uno esistente) con il contatto e, se richiesta, un'opportunita' nella prima fase aperta.
     Un commerciale vede i lead suoi e quelli senza responsabile.
    */
    public class Crm_LeadService : ServiceBase, ICrm_LeadService
    {
        private readonly ICrm_LeadRepository _Crm_Lead;
        private readonly ICrm_ClienteDatiRepository _Crm_ClienteDati;
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly ICrm_OpportunitaStoricoRepository _Crm_OpportunitaStorico;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_NotaRepository _Crm_Nota;
        private readonly ICrm_Par_FaseRepository _Crm_Par_Fase;
        private readonly ICrm_Par_FonteRepository _Crm_Par_Fonte;
        private readonly ICrmUtility _utility;
        private readonly IAz_ClienteRepository _clienteRepository;
        private readonly IAz_ClienteContattoRepository _contattoRepository;
        private readonly ILicenseGuard _licenseGuard;

        public Crm_LeadService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,
                               ICrm_LeadRepository crm_LeadRepository,
                               ICrm_ClienteDatiRepository crm_ClienteDatiRepository,
                               ICrm_OpportunitaRepository crm_OpportunitaRepository,
                               ICrm_OpportunitaStoricoRepository crm_OpportunitaStoricoRepository,
                               ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                               ICrm_AttivitaRepository crm_AttivitaRepository,
                               ICrm_NotaRepository crm_NotaRepository,
                               ICrm_Par_FaseRepository crm_Par_FaseRepository,
                               ICrm_Par_FonteRepository crm_Par_FonteRepository,
                               ICrmUtility utility,
                               IAz_ClienteRepository clienteRepository,
                               IAz_ClienteContattoRepository contattoRepository,
                               ILicenseGuard licenseGuard) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Lead = crm_LeadRepository;
            _Crm_ClienteDati = crm_ClienteDatiRepository;
            _Crm_Opportunita = crm_OpportunitaRepository;
            _Crm_OpportunitaStorico = crm_OpportunitaStoricoRepository;
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_Nota = crm_NotaRepository;
            _Crm_Par_Fase = crm_Par_FaseRepository;
            _Crm_Par_Fonte = crm_Par_FonteRepository;
            _utility = utility;
            _clienteRepository = clienteRepository;
            _contattoRepository = contattoRepository;
            _licenseGuard = licenseGuard;
        }

        public virtual async Task<GenericResult<Crm_LeadListOutModel>> Crm_LeadList(GenericRequest<Crm_LeadListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var q = _utility.LeadVisibili(ctx);
                if (!model.Data.AncheChiusi)
                    q = q.Where(x => x.Stato != Crm_StatoLead.Convertito && x.Stato != Crm_StatoLead.Scartato);
                var righe = await q.AsNoTracking().Include(x => x.Az_ClienteNavigation).OrderByDescending(x => x.CreationDate).ToListAsync();
                var nomi = await _utility.NomiAsync(righe.Select(x => x.IdAspNetUsersResponsabile));
                return new Crm_LeadListOutModel { Lead = righe.Select(x => Modello(x, nomi)).ToList() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_LeadGetOutModel>> Crm_LeadGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var lead = await _utility.LeadVisibili(ctx).AsNoTracking().Include(x => x.Az_ClienteNavigation).FirstOrDefaultAsync(x => x.Id == model.Data.Id)
                           ?? throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
                var nomi = await _utility.NomiAsync(new[] { lead.IdAspNetUsersResponsabile });
                return new Crm_LeadGetOutModel
                {
                    Lead = Modello(lead, nomi),
                    Attivita = (await CrmLists.AttivitaAsync(_Crm_Attivita.FindAll(a => a.IdCrm_Lead == lead.Id), _utility)).OrderByDescending(a => a.Inizio).ToList(),
                    Note = await CrmLists.NoteAsync(_Crm_Nota.FindAll(n => n.IdCrm_Lead == lead.Id), ctx, _utility),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_LeadPutOutModel>> Crm_LeadPut(GenericRequest<Crm_LeadPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Lead;
                if (string.IsNullOrWhiteSpace(d.Cognome) && string.IsNullOrWhiteSpace(d.Nome) && string.IsNullOrWhiteSpace(d.Azienda))
                    throw new UserMessageException(CrmMessages.Get("LeadNameRequired"));

                Crm_Lead lead;
                if (d.Id != 0)
                {
                    lead = await _utility.LeadVisibili(ctx).AsTracking().FirstOrDefaultAsync(x => x.Id == d.Id) ?? throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
                    if (lead.Stato == Crm_StatoLead.Convertito)
                        throw new UserMessageException(CrmMessages.Get("LeadAlreadyConverted"));
                }
                else
                {
                    lead = new Crm_Lead { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now, IdAspNetUsersResponsabile = ctx.Amministratore ? null : ctx.IdUtente };
                    await _Crm_Lead.CreateAsync(lead, false);
                }

                var responsabile = string.IsNullOrEmpty(d.IdResponsabile) ? null : d.IdResponsabile;
                if (responsabile != lead.IdAspNetUsersResponsabile && (d.Id != 0 || ctx.Amministratore))
                {
                    if (responsabile != null && !(await _utility.UtentiAsync(ctx)).Any(u => u.Id == responsabile))
                        throw new UserMessageException(CrmMessages.Get("UserInvalid"));
                    if (!ctx.Amministratore && !(responsabile == null || responsabile == ctx.IdUtente))
                        throw new UserMessageException(CrmMessages.Get("ResponsibleChangeNotAllowed"));
                    lead.IdAspNetUsersResponsabile = responsabile;
                }

                lead.Cognome = Pulito(d.Cognome);
                lead.Nome = Pulito(d.Nome);
                lead.Azienda = Pulito(d.Azienda);
                lead.Ruolo = Pulito(d.Ruolo);
                lead.Email = Pulito(d.Email);
                lead.Telefono = Pulito(d.Telefono);
                lead.IdCrm_Par_Fonte = d.IdCrm_Par_Fonte is int f && await _Crm_Par_Fonte.FindAll(_ => true).AnyAsync(x => x.Id == f && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica) ? f : null;
                // "Convertito" si ottiene solo con la conversione
                lead.Stato = d.Stato == Crm_StatoLead.Convertito ? lead.Stato : d.Stato;
                lead.Note = Pulito(d.Note);
                if (d.ConsensoMarketing && !lead.ConsensoMarketing)
                    lead.DataConsenso = DateTime.Now;
                if (!d.ConsensoMarketing)
                    lead.DataConsenso = null;
                lead.ConsensoMarketing = d.ConsensoMarketing;
                lead.ModifiedDate = DateTime.Now;
                lead.ChangeUser = ctx.IdUtente;
                await _Crm_Lead.SaveChange();

                var nomi = await _utility.NomiAsync(new[] { lead.IdAspNetUsersResponsabile });
                return new Crm_LeadPutOutModel { Lead = Modello(lead, nomi) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_LeadDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var lead = await _utility.LeadVisibili(ctx).AsTracking().FirstOrDefaultAsync(x => x.Id == model.Data.Id) ?? throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
                // attivita' e note del lead si cancellano con lui (FK CASCADE)
                await _Crm_Lead.DeleteAsync(lead);
                await _Crm_Lead.SaveChange();
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_LeadConvertiOutModel>> Crm_LeadConverti(GenericRequest<Crm_LeadConvertiInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data;
                var lead = await _utility.LeadVisibili(ctx).AsTracking().FirstOrDefaultAsync(x => x.Id == d.Id) ?? throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
                if (lead.Stato == Crm_StatoLead.Convertito)
                    throw new UserMessageException(CrmMessages.Get("LeadAlreadyConverted"));
                var responsabile = lead.IdAspNetUsersResponsabile ?? (ctx.Amministratore ? null : ctx.IdUtente);

                // cliente esistente (visibile) o nuovo con i dati del lead
                Az_Cliente cliente;
                if (d.IdAz_ClienteEsistente is int idEsistente)
                    cliente = await _utility.ClienteAsync(ctx, idEsistente);
                else
                {
                    var nominativo = string.Join(" ", new[] { lead.Cognome, lead.Nome }.Where(x => !string.IsNullOrWhiteSpace(x)));
                    // limite clienti della licenza CRM (ANALISI_LICENZE.md [branch Infrastructure] §5.1): il lead diventa un cliente nuovo
                    if (int.TryParse(this.CurrentCompany, out var idCompany))
                        await _licenseGuard.EnsureCanAddClientAsync(idCompany);
                    cliente = await _clienteRepository.UpsertAsync(new Az_Cliente
                    {
                        IdAz_Anagrafica = ctx.IdAz_Anagrafica,
                        Descrizione = !string.IsNullOrWhiteSpace(lead.Azienda) ? lead.Azienda! : nominativo,
                        Email = lead.Email,
                        Telefono = lead.Telefono,
                    });
                    await _Crm_ClienteDati.CreateAsync(new Crm_ClienteDati
                    {
                        IdAz_Cliente = cliente.Id, Stato = Crm_StatoCliente.Prospect, IdCrm_Par_Fonte = lead.IdCrm_Par_Fonte,
                        IdAspNetUsersResponsabile = responsabile, ConsensoMarketing = lead.ConsensoMarketing, DataConsenso = lead.DataConsenso,
                        CreationDate = DateTime.Now, ChangeUser = ctx.IdUtente
                    }, false);
                }

                // persona di riferimento (se il lead ha un nominativo)
                int? idContatto = null;
                if (!string.IsNullOrWhiteSpace(lead.Cognome) || !string.IsNullOrWhiteSpace(lead.Nome))
                {
                    bool primo = !await _Az_ClienteContatto.FindAll(_ => true).AnyAsync(x => x.IdAz_Cliente == cliente.Id);
                    var contatto = await _contattoRepository.UpsertAsync(new Az_ClienteContatto
                    {
                        IdAz_Cliente = cliente.Id, Cognome = lead.Cognome, Nome = lead.Nome, Ruolo = lead.Ruolo, Email = lead.Email,
                        Telefono = lead.Telefono, Principale = primo
                    });
                    idContatto = contatto.Id;
                }

                // opportunita' nella prima fase aperta
                int? idOpportunita = null;
                if (d.CreaOpportunita)
                {
                    var fase = await _Crm_Par_Fase.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attivo && x.Tipo == Crm_FaseTipo.Aperta)
                                        .OrderBy(x => x.Ordine).FirstAsync();
                    var opp = new Crm_Opportunita
                    {
                        IdAz_Anagrafica = ctx.IdAz_Anagrafica, Titolo = string.IsNullOrWhiteSpace(d.TitoloOpportunita) ? cliente.Descrizione : d.TitoloOpportunita.Trim(),
                        IdAz_Cliente = cliente.Id, IdAz_ClienteContatto = idContatto, IdCrm_Par_Fase = fase.Id, Valore = d.ValoreOpportunita,
                        Probabilita = fase.Probabilita, IdAspNetUsersResponsabile = responsabile, IdCrm_Par_Fonte = lead.IdCrm_Par_Fonte,
                        IdCrm_Lead = lead.Id, CreationDate = DateTime.Now, ChangeUser = ctx.IdUtente
                    };
                    await _Crm_Opportunita.CreateAsync(opp, false);
                    await _Crm_Lead.SaveChange();
                    await _Crm_OpportunitaStorico.CreateAsync(new Crm_OpportunitaStorico
                    {
                        IdCrm_Opportunita = opp.Id, Data = DateTime.Now, IdAspNetUsers = ctx.IdUtente, IdCrm_Par_FaseA = fase.Id, ValoreA = opp.Valore, CreationDate = DateTime.Now
                    }, false);
                    idOpportunita = opp.Id;
                }

                // attivita' e note del lead passano al cliente
                await _Crm_Attivita.FindAll(a => a.IdCrm_Lead == lead.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.IdAz_Cliente, cliente.Id));
                await _Crm_Nota.FindAll(n => n.IdCrm_Lead == lead.Id).ExecuteUpdateAsync(s => s.SetProperty(n => n.IdAz_Cliente, cliente.Id));

                lead.Stato = Crm_StatoLead.Convertito;
                lead.IdAz_Cliente = cliente.Id;
                lead.DataConversione = DateTime.Now;
                lead.ModifiedDate = DateTime.Now;
                lead.ChangeUser = ctx.IdUtente;
                await _Crm_Lead.SaveChange();
                return new Crm_LeadConvertiOutModel { IdAz_Cliente = cliente.Id, IdCrm_Opportunita = idOpportunita };
            }, isSubProcess);
        }

        private static Crm_LeadModel Modello(Crm_Lead x, Dictionary<string, string> nomi) => new()
        {
            Id = x.Id, Cognome = x.Cognome, Nome = x.Nome, Azienda = x.Azienda, Ruolo = x.Ruolo, Email = x.Email, Telefono = x.Telefono,
            IdCrm_Par_Fonte = x.IdCrm_Par_Fonte, Stato = x.Stato, IdResponsabile = x.IdAspNetUsersResponsabile,
            Responsabile = x.IdAspNetUsersResponsabile != null && nomi.TryGetValue(x.IdAspNetUsersResponsabile, out var n) ? n : null,
            Note = x.Note, ConsensoMarketing = x.ConsensoMarketing, DataConsenso = x.DataConsenso, IdAz_Cliente = x.IdAz_Cliente,
            Cliente = x.Az_ClienteNavigation?.Descrizione, DataConversione = x.DataConversione, CreationDate = x.CreationDate
        };

        private static string? Pulito(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public interface ICrm_LeadService : IServiceBase
    {
        Task<GenericResult<Crm_LeadListOutModel>> Crm_LeadList(GenericRequest<Crm_LeadListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_LeadGetOutModel>> Crm_LeadGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_LeadPutOutModel>> Crm_LeadPut(GenericRequest<Crm_LeadPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_LeadDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_LeadConvertiOutModel>> Crm_LeadConverti(GenericRequest<Crm_LeadConvertiInModel> model, Boolean isSubProcess);
    }
}
