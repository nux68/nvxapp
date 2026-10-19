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
     Note spese: lettura delle spese di una richiesta, ricerca per il CompanyPowerAdmin, allegati.
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
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _dip_GG_NotaSpesa_AllegatoRepository = dip_GG_NotaSpesa_AllegatoRepository;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
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
                                ?? throw new Exception("Richiesta non trovata.");
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

                if (!await HaRuolo("CompanyPowerAdmin"))
                    throw new Exception("Esame delle note spese riservato al CompanyPowerAdmin.");

                // rapporti dei dipendenti richiesti (quelli della selezione reparto/dipendente)
                var anagrafiche = await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(model.Data.UsersId ?? new List<string>());
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
                               ?? throw new Exception("Allegato non trovato.");
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
                throw new Exception("Nota spese non accessibile.");
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
        /// Controlla le spese di una nota spese in invio: almeno una spesa, importi positivi, tipi dell'azienda,
        /// allegati ammessi per tipo e dimensione. Ritorna il messaggio d'errore, null se e' tutto a posto.
        /// </summary>
        public static string? ValidaNotaSpesa(Dip_GG_Richiesta_Body_NotaSpesa? nota, ICollection<int> tipiSpesaAzienda)
        {
            if (nota?.Spese == null || nota.Spese.Count == 0)
                return "La nota spese non contiene spese.";
            if ((nota.Descrizione ?? string.Empty).Length > 200)
                return "La descrizione della nota spese supera i 200 caratteri.";

            long totaleAllegati = 0;
            foreach (var spesa in nota.Spese)
            {
                if (spesa.Importo <= 0 || spesa.Importo > 99999999)
                    return $"Importo non valido nella spesa del {spesa.Data:dd/MM/yyyy}.";
                if (!tipiSpesaAzienda.Contains(spesa.IdPar_TipoSpesa))
                    return $"Tipo di spesa non valido nella spesa del {spesa.Data:dd/MM/yyyy}.";
                if ((spesa.Descrizione ?? string.Empty).Length > 200)
                    return $"La descrizione della spesa del {spesa.Data:dd/MM/yyyy} supera i 200 caratteri.";

                var allegati = spesa.Allegati ?? new List<Dip_GG_Richiesta_Body_NotaSpesa_Allegato>();
                if (allegati.Count > AllegatiMaxPerSpesa)
                    return $"Al massimo {AllegatiMaxPerSpesa} allegati per spesa.";
                foreach (var allegato in allegati)
                {
                    // allegato gia' salvato (modifica della nota): si tiene com'e', lo controlla chi aggiorna
                    if (allegato.Id != null && allegato.Id > 0)
                        continue;
                    if (!AllegatoTipiAmmessi.Contains((allegato.ContentType ?? string.Empty).ToLowerInvariant()))
                        return $"Allegato '{allegato.NomeFile}': sono ammessi solo PDF e immagini.";
                    byte[] contenuto;
                    try { contenuto = Convert.FromBase64String(allegato.Contenuto ?? string.Empty); }
                    catch { return $"Allegato '{allegato.NomeFile}' non leggibile."; }
                    if (contenuto.Length == 0)
                        return $"Allegato '{allegato.NomeFile}' vuoto.";
                    if (contenuto.Length > AllegatoMaxByte)
                        return $"Allegato '{allegato.NomeFile}' oltre i {AllegatoMaxByte / (1024 * 1024)} MB.";
                    totaleAllegati += contenuto.Length;
                }
            }
            if (totaleAllegati > AllegatiMaxByteRichiesta)
                return $"Allegati della nota spese oltre i {AllegatiMaxByteRichiesta / (1024 * 1024)} MB complessivi.";

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
