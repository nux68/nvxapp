using nvxapp.server.data.Localization;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService
{
    /*
     Note spese: lettura delle spese di una richiesta, ricerca per il CompanyPowerAdmin (tutti i dipendenti)
     e per il CompanyAdmin (dipendenti dei reparti che amministra), allegati.
     L'invio passa dal ciclo standard delle richieste (Dip_GG_RichiestaService.Send, tipo NotaSpesa):
     la richiesta e' il contenitore, le spese sono i dettagli e ne seguono lo stato (approvazione compresa).
    */
    public class Dip_GG_NotaSpesaService : ServiceBase, IDip_GG_NotaSpesaService
    {
        // limiti degli allegati (anche il client li controlla)
        public const int AllegatoMaxByte = 5 * 1024 * 1024;
        public const int AllegatiMaxPerSpesa = 10;
        public const int AllegatiMaxByteRichiesta = 15 * 1024 * 1024;
        public static readonly string[] AllegatoTipiAmmessi = { "application/pdf", "image/jpeg", "image/png", "image/webp", "image/heic", "image/heif" };

        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;
        private readonly IDip_GG_NotaSpesa_AllegatoRepository _dip_GG_NotaSpesa_AllegatoRepository;
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IDip_RapportoAutovettureRepository _dip_RapportoAutovettureRepository;

        public Dip_GG_NotaSpesaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository,
                                  IDip_GG_NotaSpesa_AllegatoRepository dip_GG_NotaSpesa_AllegatoRepository,
                                  IDip_GG_RichiestaRepository dip_GG_RichiestaRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                  IDip_RapportoAutovettureRepository dip_RapportoAutovettureRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _dip_GG_NotaSpesa_AllegatoRepository = dip_GG_NotaSpesa_AllegatoRepository;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _dip_RapportoAutovettureRepository = dip_RapportoAutovettureRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_NotaSpesaOutModel>> GetAll(GenericRequest<Dip_GG_NotaSpesaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await Task.Delay(DelayAsyncMethod);
                return new Dip_GG_NotaSpesaOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_NotaSpesa_Get4Richiesta_OutModel>> Get4Richiesta(GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_NotaSpesa_Get4Richiesta_OutModel();

                var richiesta = _dip_GG_RichiestaRepository.FindAll(x => x.Id == model.Data.IdDip_GG_Richiesta).FirstOrDefault()
                                ?? throw new UserMessageException(AttendanceTrackingMessages.Get("RequestNotFound"));
                await VerificaAccesso(richiesta.IdDip_RapportoLavoro);

                var spese = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_GG_Richiesta == richiesta.Id).OrderBy(x => x.Data).ThenBy(x => x.Id).ToList();
                retVal.Dip_GG_NotaSpesa = ToModels(spese, new List<Dip_GG_Richiesta> { richiesta });
                retVal.Dip_GG_Richiesta = _mapper.Map<Dip_GG_RichiestaModel>(richiesta);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_NotaSpesa_Search_OutModel>> Search(GenericRequest<Dip_GG_NotaSpesa_Search_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_NotaSpesa_Search_OutModel();

                // dipendenti richiesti (quelli della selezione reparto/dipendente): il CompanyPowerAdmin
                // li vede tutti, il CompanyAdmin solo quelli dei reparti che amministra
                var usersId = model.Data.UsersId ?? new List<string>();
                if (!await HaRuolo("CompanyPowerAdmin"))
                {
                    if (!await HaRuolo("CompanyAdmin"))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ExpenseReviewAdminsOnly"));
                    var dipendentiAmministrati = DipendentiAmministrati(this.CurrentUserId);
                    usersId = usersId.Where(u => dipendentiAmministrati.Contains(u)).ToList();
                }

                // rapporti dei dipendenti richiesti
                var anagrafiche = await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(usersId);
                var idAnagrafiche = anagrafiche.Select(a => a.Id).ToList();
                var idRapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafiche.Contains(r.IdDip_Anagrafica)).Select(r => r.Id).ToList();

                var dal = model.Data.Dal.Date;
                var al = model.Data.Al.Date;
                var stati = model.Data.Stati;
                var query = _dip_GG_NotaSpesaRepository.FindAll(x => idRapporti.Contains(x.IdDip_RapportoLavoro) && x.Data >= dal && x.Data <= al);
                if (model.Data.IdPar_TipoSpesa != null && model.Data.IdPar_TipoSpesa > 0)
                    query = query.Where(x => x.IdPar_TipoSpesa == model.Data.IdPar_TipoSpesa);
                if (stati != null && stati.Count > 0)
                    query = query.Where(x => stati.Contains(x.RichiestaStato));

                var spese = query.OrderBy(x => x.Data).ThenBy(x => x.Id).ToList();
                var idRichieste = spese.Where(x => x.IdDip_GG_Richiesta != null).Select(x => x.IdDip_GG_Richiesta!.Value).Distinct().ToList();
                var richieste = _dip_GG_RichiestaRepository.FindAll(x => idRichieste.Contains(x.Id)).ToList();

                retVal.Dip_GG_NotaSpesa = ToModels(spese, richieste);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_NotaSpesa_Allegato_Get_OutModel>> AllegatoGet(GenericRequest<Dip_GG_NotaSpesa_Allegato_Get_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_NotaSpesa_Allegato_Get_OutModel();

                var allegato = _dip_GG_NotaSpesa_AllegatoRepository.FindAll(x => x.Id == model.Data.Id).FirstOrDefault()
                               ?? throw new UserMessageException(AttendanceTrackingMessages.Get("AttachmentNotFound"));
                var spesa = _dip_GG_NotaSpesaRepository.FindAll(x => x.Id == allegato.IdDip_GG_NotaSpesa).First();
                await VerificaAccesso(spesa.IdDip_RapportoLavoro);

                retVal.NomeFile = allegato.NomeFile;
                retVal.ContentType = allegato.ContentType;
                retVal.Contenuto = Convert.ToBase64String(allegato.Contenuto);

                return retVal;
            }, isSubProcess);
        }

        // le spese si vedono dal dipendente che le ha chieste e dagli amministratori dell'azienda
        private async Task VerificaAccesso(int idDip_RapportoLavoro)
        {
            if (await HaRuolo("CompanyPowerAdmin") || await HaRuolo("CompanyAdmin"))
                return;

            var anagrafica = _dip_AnagraficaRepository.FindAll(a => a.IdAspNetUsers == this.CurrentUserId).FirstOrDefault();
            bool proprio = anagrafica != null && _dip_RapportoLavoroRepository.FindAll(r => r.Id == idDip_RapportoLavoro && r.IdDip_Anagrafica == anagrafica.Id).Any();
            if (!proprio)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ExpenseReportNotAccessible"));
        }

        // dipendenti dei reparti in cui l'utente e' amministratore (come i reparti di Get4Admin)
        private HashSet<string> DipendentiAmministrati(string idAspNetUsers)
        {
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.EnabledToAdmin)
                                                       .Select(x => x.IdAz_SediReparto).ToList();
            return _az_SediRepartoUserRepository.FindAll(x => reparti.Contains(x.IdAz_SediReparto) && x.UserInDepartment)
                                                .Select(x => x.IdAspNetUsers).ToHashSet();
        }

        private async Task<bool> HaRuolo(string ruolo)
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            return user != null && await _userManager.IsInRoleAsync(user, ruolo);
        }

        private List<Dip_GG_NotaSpesaModel> ToModels(List<Dip_GG_NotaSpesa> spese, List<Dip_GG_Richiesta> richieste)
        {
            var idSpese = spese.Select(x => x.Id).ToList();
            // allegati senza contenuto
            var allegati = _dip_GG_NotaSpesa_AllegatoRepository.FindAll(x => idSpese.Contains(x.IdDip_GG_NotaSpesa))
                .Select(x => new { x.Id, x.IdDip_GG_NotaSpesa, x.NomeFile, x.ContentType, x.Dimensione })
                .ToList();

            var idRapporti = spese.Select(x => x.IdDip_RapportoLavoro).Distinct().ToList();
            var utenti = (from r in _dip_RapportoLavoroRepository.FindAll(r => idRapporti.Contains(r.Id))
                          join a in _dip_AnagraficaRepository.FindAll(a => true) on r.IdDip_Anagrafica equals a.Id
                          select new { r.Id, a.IdAspNetUsers }).ToList();

            var idAutovetture = spese.Where(x => x.IdDip_RapportoAutovetture != null).Select(x => x.IdDip_RapportoAutovetture!.Value).Distinct().ToList();
            var autovetture = _dip_RapportoAutovettureRepository.FindAll(x => idAutovetture.Contains(x.Id)).ToList();

            return spese.Select(s => new Dip_GG_NotaSpesaModel
            {
                Id = s.Id,
                IdDip_RapportoLavoro = s.IdDip_RapportoLavoro,
                IdAspNetUsers = utenti.FirstOrDefault(u => u.Id == s.IdDip_RapportoLavoro)?.IdAspNetUsers ?? string.Empty,
                IdDip_GG_Richiesta = s.IdDip_GG_Richiesta,
                RichiestaDescrizione = DescrizioneRichiesta(richieste.FirstOrDefault(r => r.Id == s.IdDip_GG_Richiesta)),
                Data = s.Data,
                IdPar_TipoSpesa = s.IdPar_TipoSpesa,
                Importo = s.Importo,
                Descrizione = s.Descrizione,
                Km = s.Km,
                IdDip_RapportoAutovetture = s.IdDip_RapportoAutovetture,
                AutovetturaDescrizione = autovetture.FirstOrDefault(a => a.Id == s.IdDip_RapportoAutovetture)?.Descrizione,
                AutovetturaTarga = autovetture.FirstOrDefault(a => a.Id == s.IdDip_RapportoAutovetture)?.Targa,
                RichiestaStato = s.RichiestaStato,
                Allegati = allegati.Where(a => a.IdDip_GG_NotaSpesa == s.Id)
                                   .Select(a => new Dip_GG_NotaSpesa_AllegatoModel { Id = a.Id, NomeFile = a.NomeFile, ContentType = a.ContentType, Dimensione = a.Dimensione })
                                   .ToList()
            }).ToList();
        }

        private static string DescrizioneRichiesta(Dip_GG_Richiesta? richiesta)
        {
            if (richiesta?.Dati == null)
                return string.Empty;
            try { return JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_NotaSpesa>(richiesta.Dati)?.Descrizione ?? string.Empty; }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Spese di rimborso km: l'importo non si prende dal client, si calcola (km x rimborso per km dell'autovettura,
        /// che deve essere del rapporto di lavoro della nota). Per gli altri tipi km e autovettura si tolgono.
        /// Va chiamata prima di ValidaNotaSpesa. Ritorna il messaggio d'errore, null se e' tutto a posto.
        /// </summary>
        public static string? CalcolaRimborsiKm(Dip_GG_Richiesta_Body_NotaSpesa? nota, ICollection<int> tipiSpesaKm, ICollection<Dip_RapportoAutovetture> autovettureRapporto)
        {
            if (nota?.Spese == null)
                return null;

            foreach (var spesa in nota.Spese)
            {
                if (!tipiSpesaKm.Contains(spesa.IdPar_TipoSpesa))
                {
                    spesa.Km = null;
                    spesa.IdDip_RapportoAutovetture = null;
                    continue;
                }

                var autovettura = autovettureRapporto.FirstOrDefault(a => a.Id == spesa.IdDip_RapportoAutovetture);
                if (autovettura == null)
                    return AttendanceTrackingMessages.Get("ExpenseMileageCarRequired", spesa.Data);
                if (spesa.Km == null || spesa.Km <= 0 || spesa.Km > 999999)
                    return AttendanceTrackingMessages.Get("ExpenseMileageInvalidKm", spesa.Data);

                spesa.Km = Math.Round(spesa.Km.Value, 2);
                spesa.Importo = Math.Round(spesa.Km.Value * autovettura.RimborsoKM, 2);
            }
            return null;
        }

        /// <summary>
        /// Controlla le spese di una nota spese in invio: almeno una spesa, importi positivi, tipi dell'azienda,
        /// allegati ammessi per tipo e dimensione. Ritorna il messaggio d'errore, null se e' tutto a posto.
        /// </summary>
        public static string? ValidaNotaSpesa(Dip_GG_Richiesta_Body_NotaSpesa? nota, ICollection<int> tipiSpesaAzienda)
        {
            if (nota?.Spese == null || nota.Spese.Count == 0)
                return AttendanceTrackingMessages.Get("ExpenseReportEmpty");
            if ((nota.Descrizione ?? string.Empty).Length > 200)
                return AttendanceTrackingMessages.Get("ExpenseReportDescriptionTooLong");

            long totaleAllegati = 0;
            foreach (var spesa in nota.Spese)
            {
                if (spesa.Importo <= 0 || spesa.Importo > 99999999)
                    return AttendanceTrackingMessages.Get("ExpenseInvalidAmount", spesa.Data);
                if (!tipiSpesaAzienda.Contains(spesa.IdPar_TipoSpesa))
                    return AttendanceTrackingMessages.Get("ExpenseInvalidType", spesa.Data);
                if ((spesa.Descrizione ?? string.Empty).Length > 200)
                    return AttendanceTrackingMessages.Get("ExpenseDescriptionTooLong", spesa.Data);

                var allegati = spesa.Allegati ?? new List<Dip_GG_Richiesta_Body_NotaSpesa_Allegato>();
                if (allegati.Count > AllegatiMaxPerSpesa)
                    return AttendanceTrackingMessages.Get("ExpenseTooManyAttachments", AllegatiMaxPerSpesa);
                foreach (var allegato in allegati)
                {
                    // allegato gia' salvato (modifica della nota): si tiene com'e', lo controlla chi aggiorna
                    if (allegato.Id != null && allegato.Id > 0)
                        continue;
                    if (!AllegatoTipiAmmessi.Contains((allegato.ContentType ?? string.Empty).ToLowerInvariant()))
                        return AttendanceTrackingMessages.Get("AttachmentTypeNotAllowed", allegato.NomeFile);
                    byte[] contenuto;
                    try { contenuto = Convert.FromBase64String(allegato.Contenuto ?? string.Empty); }
                    catch { return AttendanceTrackingMessages.Get("AttachmentUnreadable", allegato.NomeFile); }
                    if (contenuto.Length == 0)
                        return AttendanceTrackingMessages.Get("AttachmentEmpty", allegato.NomeFile);
                    if (contenuto.Length > AllegatoMaxByte)
                        return AttendanceTrackingMessages.Get("AttachmentTooLarge", allegato.NomeFile, AllegatoMaxByte / (1024 * 1024));
                    totaleAllegati += contenuto.Length;
                }
            }
            if (totaleAllegati > AllegatiMaxByteRichiesta)
                return AttendanceTrackingMessages.Get("ExpenseReportAttachmentsTooLarge", AllegatiMaxByteRichiesta / (1024 * 1024));

            return null;
        }
    }

    public interface IDip_GG_NotaSpesaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_NotaSpesaOutModel>> GetAll( GenericRequest<Dip_GG_NotaSpesaInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_NotaSpesa_Get4Richiesta_OutModel>> Get4Richiesta(GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_NotaSpesa_Search_OutModel>> Search(GenericRequest<Dip_GG_NotaSpesa_Search_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_NotaSpesa_Allegato_Get_OutModel>> AllegatoGet(GenericRequest<Dip_GG_NotaSpesa_Allegato_Get_InModel> model, Boolean isSubProcess);
    }
}
