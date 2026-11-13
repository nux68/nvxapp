using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService
{
    /*
     Accordi individuali di lavoro agile e calendario (AT-SW.1/2, ANALISI_ATTENDANCETRACKING_SMARTWORKING.md):
       - accordi: CompanyAdmin/CompanyPowerAdmin; al salvataggio si chiede la presa visione al dipendente con
         IAcknowledgementService (fonte "at.accordo-sw": con il Documentale attivo il canale di firma genera il verbale di accordo)
       - Calendario: giorni consentiti, giorni richiesti/approvati e conteggio settimanale (dipendente o HR)
     I giorni si richiedono con la richiesta di giustificativo (giustificativo LavoroAgile): il controllo e' in
     Dip_GG_RichiestaService.Send (ILavoroAgileUtility.ValidaRichiestaAsync).
    */
    public class Dip_AccordoSWService : ServiceBase, IDip_AccordoSWService
    {
        private readonly ITurniUtility _turniUtility;
        private readonly ILavoroAgileUtility _lavoroAgileUtility;
        private readonly IDip_AccordoSWRepository _dip_AccordoSWRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IAcknowledgementService _acknowledgementService;
        private readonly IGestionePresenzeOreTeoriche _gestionePresenzeOreTeoriche;
        private readonly ICompanyRepository _companyRepository;

        public Dip_AccordoSWService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,

                                    ITurniUtility turniUtility,
                                    ILavoroAgileUtility lavoroAgileUtility,
                                    IDip_AccordoSWRepository dip_AccordoSWRepository,
                                    IDip_AnagraficaRepository dip_AnagraficaRepository,
                                    IAcknowledgementService acknowledgementService,
                                    IGestionePresenzeOreTeoriche gestionePresenzeOreTeoriche,
                                    ICompanyRepository companyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _turniUtility = turniUtility;
            _lavoroAgileUtility = lavoroAgileUtility;
            _dip_AccordoSWRepository = dip_AccordoSWRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _acknowledgementService = acknowledgementService;
            _gestionePresenzeOreTeoriche = gestionePresenzeOreTeoriche;
            _companyRepository = companyRepository;
        }

        private int IdCompany => int.TryParse(CurrentCompany, out var id) ? id : 0;

        private async Task VerificaAdmin()
        {
            if (!await _turniUtility.IsAdminAsync(CurrentUserId))
                throw new UserMessageException(AttendanceTrackingMessages.Get("SwNonAutorizzato"));
        }

        private List<TurnoDipendenteModel> DipendentiAzienda(int idAzienda)
        {
            var utenti = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda && a.IdAspNetUsers != null).Select(a => a.IdAspNetUsers!).ToList();
            return _turniUtility.Dipendenti(utenti, idAzienda, new DateTime(1900, 1, 1), new DateTime(2999, 12, 31));
        }

        private async Task<Dip_AccordoSWModel> Modello(Dip_AccordoSW a, List<TurnoDipendenteModel> dipendenti)
        {
            var firmato = await _lavoroAgileUtility.FirmatoAsync(IdCompany, a);
            var dip = dipendenti.FirstOrDefault(d => d.IdDip_RapportoLavoro == a.IdDip_RapportoLavoro);
            return new Dip_AccordoSWModel
            {
                Id = a.Id,
                IdDip_RapportoLavoro = a.IdDip_RapportoLavoro,
                IdAspNetUsers = dip?.IdAspNetUsers,
                Nominativo = dip?.Nominativo,
                Dal = a.Dal,
                Al = a.Al,
                MaxGiorniSettimana = a.MaxGiorniSettimana,
                GiorniConsentiti = a.GiorniConsentiti,
                Note = a.Note,
                DataRichiestaFirma = a.DataRichiestaFirma,
                DataFirma = a.DataFirma,
                Firmato = firmato
            };
        }

        public virtual async Task<GenericResult<Dip_AccordoSW_GetAllOutModel>> GetAll(GenericRequest<Dip_AccordoSW_GetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_AccordoSW_GetAllOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                bool admin = await _turniUtility.IsAdminAsync(CurrentUserId);
                var utente = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? (admin ? null : CurrentUserId) : model.Data.IdAspNetUsers;
                if (!admin && utente != CurrentUserId)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("SwNonAutorizzato"));

                var dipendenti = DipendentiAzienda(idAzienda);
                var rapporti = dipendenti.Where(d => utente == null || d.IdAspNetUsers == utente).Select(d => d.IdDip_RapportoLavoro).ToList();
                foreach (var a in _dip_AccordoSWRepository.FindAll(a => rapporti.Contains(a.IdDip_RapportoLavoro)).OrderByDescending(a => a.Dal).ToList())
                    retVal.Dip_AccordoSW.Add(await Modello(a, dipendenti));
                if (admin)
                    retVal.Dipendenti = dipendenti;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_AccordoSW_GetOutModel>> Dip_AccordoSWGet(GenericRequest<Dip_AccordoSW_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var retVal = new Dip_AccordoSW_GetOutModel { Dipendenti = DipendentiAzienda(idAzienda) };
                var a = _dip_AccordoSWRepository.FindAll(x => x.Id == model.Data.Id).FirstOrDefault();
                if (a != null && retVal.Dipendenti.Any(d => d.IdDip_RapportoLavoro == a.IdDip_RapportoLavoro))
                    retVal.Dip_AccordoSW = await Modello(a, retVal.Dipendenti);
                else
                {
                    var oggi = DateTime.Today;
                    retVal.Dip_AccordoSW = new Dip_AccordoSWModel { Dal = oggi, Al = new DateTime(oggi.Year, 12, 31) };
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_AccordoSW_PutOutModel>> Dip_AccordoSWPut(GenericRequest<Dip_AccordoSW_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var m = model.Data.Dip_AccordoSW;
                var dipendenti = DipendentiAzienda(idAzienda);
                var dip = dipendenti.FirstOrDefault(d => d.IdAspNetUsers == m.IdAspNetUsers)
                          ?? throw new UserMessageException(AttendanceTrackingMessages.Get("SwDipendenteNonValido"));

                var dal = m.Dal.Date;
                var al = m.Al.Date;
                var maschera = (m.GiorniConsentiti ?? string.Empty).Trim();
                if (al < dal)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("SwPeriodoNonValido"));
                if (m.MaxGiorniSettimana < 1 || m.MaxGiorniSettimana > 7)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("SwMaxGiorniNonValido"));
                if (maschera.Length != 7 || maschera.Any(c => c != '0' && c != '1') || !maschera.Contains('1'))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("SwGiorniNonValidi"));
                if (_dip_AccordoSWRepository.FindAll(a => a.IdDip_RapportoLavoro == dip.IdDip_RapportoLavoro && a.Id != m.Id && a.Dal <= al && a.Al >= dal).Any())
                    throw new UserMessageException(AttendanceTrackingMessages.Get("SwAccordoSovrapposto"));

                var accordo = m.Id > 0 ? _dip_AccordoSWRepository.FindAll(a => a.Id == m.Id && a.IdDip_RapportoLavoro == dip.IdDip_RapportoLavoro).FirstOrDefault() : null;
                bool nuovo = accordo == null;
                // condizioni cambiate: si chiede di nuovo la presa visione
                bool cambiato = !nuovo && (accordo!.Dal != dal || accordo.Al != al || accordo.MaxGiorniSettimana != m.MaxGiorniSettimana || accordo.GiorniConsentiti != maschera);
                accordo ??= new Dip_AccordoSW { IdDip_RapportoLavoro = dip.IdDip_RapportoLavoro };
                accordo.Dal = dal;
                accordo.Al = al;
                accordo.MaxGiorniSettimana = m.MaxGiorniSettimana;
                accordo.GiorniConsentiti = maschera;
                accordo.Note = string.IsNullOrWhiteSpace(m.Note) ? null : m.Note.Trim();
                if (nuovo || cambiato)
                {
                    accordo.DataFirma = null;
                    accordo.DataRichiestaFirma = DateTime.Now;
                }
                accordo = await _dip_AccordoSWRepository.UpsertAsync(accordo);

                // il giustificativo del lavoro agile deve esistere per richiedere i giorni (aziende create prima di AT-SW)
                await _lavoroAgileUtility.GetOrCreateGiustificativoAsync(idAzienda);

                if (nuovo || cambiato)
                {
                    var giorni = string.Join(", ", Enumerable.Range(0, 7).Where(i => maschera[i] == '1').Select(i => AttendanceTrackingMessages.Get("SwGiorno" + i)));
                    await _acknowledgementService.RequestAsync(new AcknowledgementRequest
                    {
                        IdCompany = IdCompany,
                        Application = ApplicationType.AttendanceTracking,
                        Fonte = LavoroAgileUtility.FontePresaVisione,
                        IdRiferimento = accordo.Id,
                        UserIds = new[] { dip.IdAspNetUsers },
                        Titolo = AttendanceTrackingMessages.Get("SwPresaVisioneTitolo", dal, al),
                        Testo = AttendanceTrackingMessages.Get("SwPresaVisioneTesto", dip.Nominativo, dal, al, accordo.MaxGiorniSettimana, giorni)
                                + (accordo.Note != null ? "\n" + accordo.Note : string.Empty),
                        Url = "/lavoroagile",
                        RichiediDiNuovo = cambiato
                    });
                }

                return new Dip_AccordoSW_PutOutModel { Dip_AccordoSW = await Modello(accordo, dipendenti) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_AccordoSW_DeleteOutModel>> Dip_AccordoSWDelete(GenericRequest<Dip_AccordoSW_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaAdmin();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var a = _dip_AccordoSWRepository.FindAll(x => x.Id == model.Data.Id).FirstOrDefault();
                if (a != null && DipendentiAzienda(idAzienda).Any(d => d.IdDip_RapportoLavoro == a.IdDip_RapportoLavoro))
                {
                    await _acknowledgementService.CancelAsync(IdCompany, ApplicationType.AttendanceTracking, LavoroAgileUtility.FontePresaVisione, a.Id);
                    await _dip_AccordoSWRepository.DeleteAsync(a);
                }
                return new Dip_AccordoSW_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_AccordoSW_CalendarioOutModel>> Calendario(GenericRequest<Dip_AccordoSW_CalendarioInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_AccordoSW_CalendarioOutModel();
                int idAzienda = await _turniUtility.IdAziendaAsync(CurrentCompany);
                var utente = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? CurrentUserId : model.Data.IdAspNetUsers;
                if (utente != CurrentUserId)
                    await VerificaAdmin();

                var dal = new DateTime(model.Data.Year, model.Data.Month, 1);
                var al = dal.AddMonths(1).AddDays(-1);
                retVal.IdPar_Giustificativi = (await _lavoroAgileUtility.GetOrCreateGiustificativoAsync(idAzienda)).Id;
                retVal.Azienda = _companyRepository.FindAll(c => c.Id == IdCompany).Select(c => c.Descrizione).FirstOrDefault() ?? string.Empty;
                retVal.UserName = _aspNetUsersRepository.FindAll(u => u.Id == utente).Select(u => u.UserName).FirstOrDefault() ?? string.Empty;

                var dip = _turniUtility.Dipendenti(new List<string> { utente }, idAzienda, dal, al).FirstOrDefault();
                if (dip == null)
                    return retVal;

                var dipendenti = new List<TurnoDipendenteModel> { dip };
                var accordi = _dip_AccordoSWRepository.FindAll(a => a.IdDip_RapportoLavoro == dip.IdDip_RapportoLavoro && a.Dal <= al.AddDays(6) && a.Al >= dal.AddDays(-6)).ToList();
                foreach (var a in accordi)
                    retVal.Accordi.Add(await Modello(a, dipendenti));

                var teoriche = _gestionePresenzeOreTeoriche.GetOreTeoriche(dip.IdDip_RapportoLavoro, dal, al);
                var inizioSett = LavoroAgileUtility.Lunedi(dal);
                var fineSett = LavoroAgileUtility.Lunedi(al).AddDays(7);
                var giorniSW = _lavoroAgileUtility.Giorni(dip.IdDip_RapportoLavoro, inizioSett, fineSett);

                for (var d = dal; d <= al; d = d.AddDays(1))
                {
                    var accordo = retVal.Accordi.FirstOrDefault(a => a.Dal.Date <= d && a.Al.Date >= d);
                    var sw = giorniSW.Where(g => g.Data == d).Select(g => ((StatoRichiesta Stato, int? Id)?)(g.Stato, g.IdDip_GG_Richiesta)).FirstOrDefault();
                    retVal.Giorni.Add(new LavoroAgileGiornoModel
                    {
                        Data = d,
                        Lavorativo = teoriche.TryGetValue(d, out var ore) && ore > TimeSpan.Zero,
                        Consentito = accordo != null && accordo.Firmato && LavoroAgileUtility.GiornoConsentito(accordo.GiorniConsentiti, d),
                        Stato = sw?.Stato,
                        IdDip_GG_Richiesta = sw?.Id
                    });
                }

                for (var lun = inizioSett; lun < fineSett; lun = lun.AddDays(7))
                {
                    var accordo = retVal.Accordi.FirstOrDefault(a => a.Dal.Date <= lun.AddDays(6) && a.Al.Date >= lun);
                    retVal.Settimane.Add(new LavoroAgileSettimanaModel
                    {
                        Lunedi = lun,
                        Usati = giorniSW.Count(g => LavoroAgileUtility.Lunedi(g.Data) == lun),
                        Max = accordo?.MaxGiorniSettimana ?? 0
                    });
                }
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDip_AccordoSWService : IServiceBase
    {
        Task<GenericResult<Dip_AccordoSW_GetAllOutModel>> GetAll(GenericRequest<Dip_AccordoSW_GetAllInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_AccordoSW_GetOutModel>> Dip_AccordoSWGet(GenericRequest<Dip_AccordoSW_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_AccordoSW_PutOutModel>> Dip_AccordoSWPut(GenericRequest<Dip_AccordoSW_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_AccordoSW_DeleteOutModel>> Dip_AccordoSWDelete(GenericRequest<Dip_AccordoSW_DeleteInModel> model, bool isSubProcess);
        Task<GenericResult<Dip_AccordoSW_CalendarioOutModel>> Calendario(GenericRequest<Dip_AccordoSW_CalendarioInModel> model, bool isSubProcess);
    }
}
