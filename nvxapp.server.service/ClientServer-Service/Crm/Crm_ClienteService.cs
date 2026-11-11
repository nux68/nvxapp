using AutoMapper;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using ClosedXML.Excel;
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
     Clienti nel CRM (ANALISI_CRM_APPLICATIVO.md §6.1): anagrafica comune (Az_Cliente, scritta con i repository di Infrastructure)
     + dati del CRM (Crm_ClienteDati, tag) + scheda con contatti, opportunita', attivita', offerte, note e allegati.
     Ognuno lavora sui clienti che vede (CrmUtility.ClientiVisibili); un commerciale che crea un cliente ne diventa
     responsabile, e solo gli amministratori cambiano il responsabile di un cliente gia' assegnato ad altri.
     La cancellazione dei clienti resta nella pagina comune (Anagrafiche > Clienti).
    */
    public class Crm_ClienteService : ServiceBase, ICrm_ClienteService
    {
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_ClienteDatiRepository _Crm_ClienteDati;
        private readonly ICrm_ClienteTagRepository _Crm_ClienteTag;
        private readonly ICrm_AllegatoRepository _Crm_Allegato;
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_NotaRepository _Crm_Nota;
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly ICrm_Par_FonteRepository _Crm_Par_Fonte;
        private readonly ICrm_Par_TagRepository _Crm_Par_Tag;
        private readonly ICrmUtility _utility;
        private readonly IAz_ClienteRepository _clienteRepository;
        private readonly IAz_ClienteContattoRepository _contattoRepository;
        private readonly ILicenseGuard _licenseGuard;

        public Crm_ClienteService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  ICrm_ClienteDatiRepository crm_ClienteDatiRepository,
                                  ICrm_ClienteTagRepository crm_ClienteTagRepository,
                                  ICrm_AllegatoRepository crm_AllegatoRepository,
                                  ICrm_AttivitaRepository crm_AttivitaRepository,
                                  ICrm_NotaRepository crm_NotaRepository,
                                  ICrm_OpportunitaRepository crm_OpportunitaRepository,
                                  ICrm_Par_FonteRepository crm_Par_FonteRepository,
                                  ICrm_Par_TagRepository crm_Par_TagRepository,
                                  ICrmUtility utility,
                                  IAz_ClienteRepository clienteRepository,
                                  IAz_ClienteContattoRepository contattoRepository,
                                  ILicenseGuard licenseGuard) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _Crm_ClienteDati = crm_ClienteDatiRepository;
            _Crm_ClienteTag = crm_ClienteTagRepository;
            _Crm_Allegato = crm_AllegatoRepository;
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_Nota = crm_NotaRepository;
            _Crm_Opportunita = crm_OpportunitaRepository;
            _Crm_Par_Fonte = crm_Par_FonteRepository;
            _Crm_Par_Tag = crm_Par_TagRepository;
            _utility = utility;
            _clienteRepository = clienteRepository;
            _contattoRepository = contattoRepository;
            _licenseGuard = licenseGuard;
        }

        public virtual async Task<GenericResult<Crm_ClienteListOutModel>> Crm_ClienteList(GenericRequest<Crm_ClienteListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                return new Crm_ClienteListOutModel { Clienti = await ElencoAsync(ctx, model.Data) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_ClienteGetOutModel>> Crm_ClienteGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var c = await _utility.ClienteAsync(ctx, model.Data.Id);
                var dati = await _Crm_ClienteDati.FindAll(_ => true).FirstOrDefaultAsync(x => x.IdAz_Cliente == c.Id);
                var cfg = await _utility.CfgAsync(ctx);
                return new Crm_ClienteGetOutModel
                {
                    Cliente = Modello(c, dati, await _Crm_ClienteTag.FindAll(x => x.IdAz_Cliente == c.Id).Select(x => x.IdCrm_Par_Tag).ToListAsync()),
                    Contatti = await CrmLists.ContattiAsync(_Az_ClienteContatto, c.Id),
                    Opportunita = await CrmLists.OpportunitaAsync(_utility.OpportunitaVisibili(ctx).Where(o => o.IdAz_Cliente == c.Id), _utility),
                    Attivita = (await CrmLists.AttivitaAsync(_utility.AttivitaVisibili(ctx).Where(a => a.IdAz_Cliente == c.Id), _utility)).OrderByDescending(a => a.Inizio).ToList(),
                    Offerte = await CrmLists.OfferteAsync(_utility.OfferteVisibili(ctx).Where(o => o.IdAz_Cliente == c.Id), cfg),
                    Note = await CrmLists.NoteAsync(_Crm_Nota.FindAll(n => n.IdAz_Cliente == c.Id), ctx, _utility),
                    Allegati = await CrmLists.AllegatiAsync(_Crm_Allegato.FindAll(a => a.IdAz_Cliente == c.Id), _utility),
                    Modificabile = true,
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_ClientePutOutModel>> Crm_ClientePut(GenericRequest<Crm_ClientePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Cliente;
                if (string.IsNullOrWhiteSpace(d.Descrizione))
                    throw new UserMessageException(CrmMessages.Get("CustomerNameRequired"));

                Az_Cliente cliente;
                Crm_ClienteDati? dati = null;
                if (d.Id != 0)
                {
                    await _utility.ClienteAsync(ctx, d.Id); // visibile
                    cliente = (await _clienteRepository.FindByIdAsync(d.Id))!;
                    dati = await _Crm_ClienteDati.FindAll(_ => true).AsTracking().FirstOrDefaultAsync(x => x.IdAz_Cliente == d.Id);
                }
                else
                    cliente = new Az_Cliente { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                // per il limite clienti della licenza CRM: contava gia' prima della modifica?
                bool contavaPrima = d.Id != 0 && _licenseGuard.ContaComeCliente(cliente);

                // anagrafica comune
                cliente.Descrizione = d.Descrizione.Trim();
                cliente.Tipo = (TipoCliente)d.Tipo;
                cliente.Attivo = d.Attivo;
                cliente.PartitaIva = Pulito(d.PartitaIva);
                cliente.CodiceFiscale = Pulito(d.CodiceFiscale)?.ToUpperInvariant();
                cliente.Indirizzo = Pulito(d.Indirizzo);
                cliente.Cap = Pulito(d.Cap);
                cliente.Citta = Pulito(d.Citta);
                cliente.Provincia = Pulito(d.Provincia)?.ToUpperInvariant();
                cliente.Nazione = Pulito(d.Nazione)?.ToUpperInvariant();
                cliente.Telefono = Pulito(d.Telefono);
                cliente.Email = Pulito(d.Email);
                cliente.Pec = Pulito(d.Pec);
                cliente.Referente = Pulito(d.Referente);
                cliente.CodiceDestinatario = Pulito(d.CodiceDestinatario)?.ToUpperInvariant();
                cliente.Note = Pulito(d.Note);

                // limite clienti della licenza CRM (ANALISI_INFRASTRUCTURE_LICENZE.md [branch Infrastructure] §5.1): cliente nuovo, riattivato
                // o che smette di essere solo fornitore
                if (_licenseGuard.ContaComeCliente(cliente) && !contavaPrima && int.TryParse(this.CurrentCompany, out var idCompany))
                    await _licenseGuard.EnsureCanAddClientAsync(idCompany);

                cliente = await _clienteRepository.UpsertAsync(cliente);

                // dati del CRM
                if (dati == null)
                {
                    dati = new Crm_ClienteDati { IdAz_Cliente = cliente.Id, CreationDate = DateTime.Now };
                    await _Crm_ClienteDati.CreateAsync(dati, false);
                    // il commerciale che crea il cliente ne e' il responsabile
                    if (!ctx.Amministratore)
                        dati.IdAspNetUsersResponsabile = ctx.IdUtente;
                }
                var utenti = (await _utility.UtentiAsync(ctx)).Select(u => u.Id).ToHashSet();
                var responsabile = string.IsNullOrEmpty(d.IdResponsabile) ? null : d.IdResponsabile;
                // cliente nuovo di un commerciale senza responsabile indicato: resta suo
                if (d.Id == 0 && responsabile == null && !ctx.Amministratore)
                    responsabile = ctx.IdUtente;
                if (responsabile != null && !utenti.Contains(responsabile))
                    throw new UserMessageException(CrmMessages.Get("UserInvalid"));
                if (responsabile != dati.IdAspNetUsersResponsabile)
                {
                    // un commerciale puo' prendere un cliente libero o lasciarne uno suo; gli altri cambi sono degli amministratori
                    bool proprio = dati.IdAspNetUsersResponsabile == null || dati.IdAspNetUsersResponsabile == ctx.IdUtente;
                    bool versoSeOLibero = responsabile == null || responsabile == ctx.IdUtente;
                    if (!ctx.Amministratore && !(proprio && versoSeOLibero))
                        throw new UserMessageException(CrmMessages.Get("ResponsibleChangeNotAllowed"));
                    dati.IdAspNetUsersResponsabile = responsabile;
                }
                dati.Stato = d.Stato;
                dati.IdCrm_Par_Fonte = d.IdCrm_Par_Fonte is int f && await _Crm_Par_Fonte.FindAll(_ => true).AnyAsync(x => x.Id == f && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica) ? f : null;
                dati.Settore = Pulito(d.Settore);
                dati.FatturatoStimato = d.FatturatoStimato;
                dati.NumeroDipendenti = d.NumeroDipendenti;
                dati.Sito = Pulito(d.Sito);
                if (d.ConsensoMarketing && !dati.ConsensoMarketing)
                    dati.DataConsenso = DateTime.Now;
                if (!d.ConsensoMarketing)
                    dati.DataConsenso = null;
                dati.ConsensoMarketing = d.ConsensoMarketing;
                dati.ModifiedDate = DateTime.Now;
                dati.ChangeUser = ctx.IdUtente;

                // tag
                var tagValidi = await _Crm_Par_Tag.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Select(x => x.Id).ToListAsync();
                var nuovi = d.Tag.Where(tagValidi.Contains).Distinct().ToList();
                var attuali = await _Crm_ClienteTag.FindAll(_ => true).AsTracking().Where(x => x.IdAz_Cliente == cliente.Id).ToListAsync();
                await _Crm_ClienteTag.DeleteRangeAsync(attuali.Where(x => !nuovi.Contains(x.IdCrm_Par_Tag)));
                foreach (var t in nuovi.Where(t => attuali.All(a => a.IdCrm_Par_Tag != t)))
                    await _Crm_ClienteTag.CreateAsync(new Crm_ClienteTag { IdAz_Cliente = cliente.Id, IdCrm_Par_Tag = t, CreationDate = DateTime.Now }, false);

                await _Crm_ClienteDati.SaveChange();
                return new Crm_ClientePutOutModel { Cliente = Modello(cliente, dati, nuovi) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_ContattoPutOutModel>> Crm_ContattoPut(GenericRequest<Crm_ContattoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Contatto;
                if (string.IsNullOrWhiteSpace(d.Cognome) && string.IsNullOrWhiteSpace(d.Nome))
                    throw new UserMessageException(InfrastructureMessages.Get("CustomerContactNameRequired"));

                Az_ClienteContatto contatto;
                if (d.Id != 0)
                {
                    contatto = await _contattoRepository.FindByIdAsync(d.Id) ?? throw new UserMessageException(InfrastructureMessages.Get("CustomerContactNotFound"));
                    d.IdAz_Cliente = contatto.IdAz_Cliente; // il contatto non cambia cliente
                }
                else
                    contatto = new Az_ClienteContatto { IdAz_Cliente = d.IdAz_Cliente };
                await _utility.ClienteAsync(ctx, d.IdAz_Cliente); // cliente visibile

                contatto.Cognome = Pulito(d.Cognome);
                contatto.Nome = Pulito(d.Nome);
                contatto.Ruolo = Pulito(d.Ruolo);
                contatto.Telefono = Pulito(d.Telefono);
                contatto.Cellulare = Pulito(d.Cellulare);
                contatto.Email = Pulito(d.Email);
                contatto.Principale = d.Principale;
                contatto.Note = Pulito(d.Note);
                contatto = await _contattoRepository.UpsertAsync(contatto);

                if (contatto.Principale)
                    foreach (var altro in await _contattoRepository.FindAll(x => x.IdAz_Cliente == contatto.IdAz_Cliente && x.Principale && x.Id != contatto.Id).ToListAsync())
                    {
                        altro.Principale = false;
                        await _contattoRepository.UpdateAsync(altro);
                    }
                return new Crm_ContattoPutOutModel { Contatto = CrmLists.Contatto(contatto) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_ContattoDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var contatto = await _contattoRepository.FindByIdAsync(model.Data.Id) ?? throw new UserMessageException(InfrastructureMessages.Get("CustomerContactNotFound"));
                await _utility.ClienteAsync(ctx, contatto.IdAz_Cliente);
                // usato da opportunita', attivita' e offerte: il riferimento si svuota (FK SET NULL)
                await _contattoRepository.DeleteAsync(contatto);
                return new Crm_IdOutModel { Id = contatto.Id };
            }, isSubProcess);
        }

        // elenco dei clienti visibili in Excel (stessi filtri della pagina)
        public virtual async Task<Crm_FileDownload> Crm_ClienteExport(Crm_ClienteListInModel filtri)
        {
            var ctx = await _utility.ContestoAsync();
            var clienti = await ElencoAsync(ctx, filtri);
            var tag = await _Crm_Par_Tag.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.Descrizione);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(CrmMessages.Get("ExportCustomersSheet"));
            var titoli = new[] { "ExportColCustomer", "ExportColStatus", "ExportColResponsible", "ExportColCity", "ExportColEmail", "ExportColPhone",
                                 "ExportColTags", "ExportColOpenOpportunities", "ExportColOpenValue", "ExportColLastActivity", "ExportColNextActivity" };
            for (int i = 0; i < titoli.Length; i++)
                ws.Cell(1, i + 1).Value = CrmMessages.Get(titoli[i]);
            ws.Row(1).Style.Font.Bold = true;
            int r = 2;
            foreach (var c in clienti)
            {
                ws.Cell(r, 1).Value = c.Descrizione;
                ws.Cell(r, 2).Value = CrmMessages.Get("CustomerStatus" + (int)c.Stato);
                ws.Cell(r, 3).Value = c.Responsabile ?? string.Empty;
                ws.Cell(r, 4).Value = c.Citta ?? string.Empty;
                ws.Cell(r, 5).Value = c.Email ?? string.Empty;
                ws.Cell(r, 6).Value = c.Telefono ?? string.Empty;
                ws.Cell(r, 7).Value = string.Join(", ", c.Tag.Select(t => tag.TryGetValue(t, out var n) ? n : string.Empty));
                ws.Cell(r, 8).Value = c.OpportunitaAperte;
                ws.Cell(r, 9).Value = c.ValoreAperto;
                if (c.UltimaAttivita.HasValue) ws.Cell(r, 10).Value = c.UltimaAttivita.Value;
                if (c.ProssimaAttivita.HasValue) ws.Cell(r, 11).Value = c.ProssimaAttivita.Value;
                r++;
            }
            ws.Column(9).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns(10, 11).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            ws.Columns().AdjustToContents();

            var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            return new Crm_FileDownload
            {
                Content = ms,
                NomeFile = $"{CrmMessages.Get("ExportCustomersFile")}_{DateTime.Now:yyyyMMdd}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<List<Crm_ClienteListModel>> ElencoAsync(CrmContesto ctx, Crm_ClienteListInModel f)
        {
            var now = DateTime.Now;
            // sottoquery dai repository in variabili locali: EF le incorpora nella query (una chiamata a FindAll
            // scritta dentro la lambda non si traduce in SQL)
            var q = _utility.ClientiVisibili(ctx);
            var dati = _Crm_ClienteDati.FindAll(_ => true);
            var tag = _Crm_ClienteTag.FindAll(_ => true);
            var opportunitaAperte = _Crm_Opportunita.FindAll(o => o.Crm_Par_FaseNavigation!.Tipo == Crm_FaseTipo.Aperta);
            var attivita = _Crm_Attivita.FindAll(_ => true);
            var righe = await q.AsNoTracking()
                .Select(c => new
                {
                    c.Id, c.Descrizione, c.Tipo, c.Attivo, c.Citta, c.Email, c.Telefono,
                    Dati = dati.Where(d => d.IdAz_Cliente == c.Id).Select(d => new { d.Stato, d.IdAspNetUsersResponsabile }).FirstOrDefault(),
                    Tag = tag.Where(t => t.IdAz_Cliente == c.Id).Select(t => t.IdCrm_Par_Tag).ToList(),
                    Aperte = opportunitaAperte.Count(o => o.IdAz_Cliente == c.Id),
                    Valore = opportunitaAperte.Where(o => o.IdAz_Cliente == c.Id).Sum(o => (decimal?)o.Valore) ?? 0,
                    Ultima = attivita.Where(a => a.IdAz_Cliente == c.Id && a.Stato == Crm_StatoAttivita.Fatta).Max(a => (DateTime?)a.Inizio),
                    Prossima = attivita.Where(a => a.IdAz_Cliente == c.Id && a.Stato == Crm_StatoAttivita.DaFare).Min(a => (DateTime?)a.Inizio),
                })
                .ToListAsync();

            var lista = righe.Select(c => new Crm_ClienteListModel
            {
                Id = c.Id, Descrizione = c.Descrizione, Tipo = (int)c.Tipo, Attivo = c.Attivo, Citta = c.Citta, Email = c.Email, Telefono = c.Telefono,
                Stato = c.Dati?.Stato ?? Crm_StatoCliente.Prospect, IdResponsabile = c.Dati?.IdAspNetUsersResponsabile, Tag = c.Tag,
                OpportunitaAperte = c.Aperte, ValoreAperto = c.Valore, UltimaAttivita = c.Ultima, ProssimaAttivita = c.Prossima
            }).ToList();

            if (!string.IsNullOrEmpty(f.IdResponsabile))
                lista = lista.Where(x => x.IdResponsabile == f.IdResponsabile).ToList();
            if (f.Stato.HasValue)
                lista = lista.Where(x => x.Stato == f.Stato).ToList();
            if (f.IdTag.HasValue)
                lista = lista.Where(x => x.Tag.Contains(f.IdTag.Value)).ToList();

            var nomi = await _utility.NomiAsync(lista.Select(x => x.IdResponsabile));
            lista.ForEach(x => x.Responsabile = x.IdResponsabile != null && nomi.TryGetValue(x.IdResponsabile, out var n) ? n : null);
            return lista.OrderBy(x => x.Descrizione).ToList();
        }

        private static Crm_ClienteModel Modello(Az_Cliente c, Crm_ClienteDati? d, List<int> tag) => new()
        {
            Id = c.Id, Descrizione = c.Descrizione, Tipo = (int)c.Tipo, Attivo = c.Attivo, PartitaIva = c.PartitaIva, CodiceFiscale = c.CodiceFiscale,
            Indirizzo = c.Indirizzo, Cap = c.Cap, Citta = c.Citta, Provincia = c.Provincia, Nazione = c.Nazione, Telefono = c.Telefono, Email = c.Email,
            Pec = c.Pec, Referente = c.Referente, CodiceDestinatario = c.CodiceDestinatario, Note = c.Note,
            Stato = d?.Stato ?? Crm_StatoCliente.Prospect, IdCrm_Par_Fonte = d?.IdCrm_Par_Fonte, IdResponsabile = d?.IdAspNetUsersResponsabile,
            Settore = d?.Settore, FatturatoStimato = d?.FatturatoStimato, NumeroDipendenti = d?.NumeroDipendenti, Sito = d?.Sito,
            ConsensoMarketing = d?.ConsensoMarketing ?? false, DataConsenso = d?.DataConsenso, Tag = tag
        };

        private static string? Pulito(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public interface ICrm_ClienteService : IServiceBase
    {
        Task<GenericResult<Crm_ClienteListOutModel>> Crm_ClienteList(GenericRequest<Crm_ClienteListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_ClienteGetOutModel>> Crm_ClienteGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_ClientePutOutModel>> Crm_ClientePut(GenericRequest<Crm_ClientePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_ContattoPutOutModel>> Crm_ContattoPut(GenericRequest<Crm_ContattoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_ContattoDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<Crm_FileDownload> Crm_ClienteExport(Crm_ClienteListInModel filtri);
    }
}
