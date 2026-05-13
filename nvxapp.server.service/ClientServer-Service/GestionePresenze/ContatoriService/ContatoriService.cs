using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService
{
    public interface IContatoriService
    {
        Task<GenericResult<Contatori_Calcolo_OutModel>> CalcolaContatori(
            GenericRequest<Contatori_Calcolo_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Contatori_Riporto_GetAll_OutModel>> Riporto_GetAll(
            GenericRequest<Contatori_Riporto_GetAll_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Contatori_Riporto_Upsert_OutModel>> Riporto_Upsert(
            GenericRequest<Contatori_Riporto_Upsert_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Contatori_Riporto_Delete_OutModel>> Riporto_Delete(
            GenericRequest<Contatori_Riporto_Delete_InModel> model,
            bool isSubProcess = false);

        Task<GenericResult<Contatori_Anno_OutModel>> CalcolaContatori_Anno(
            GenericRequest<Contatori_Anno_InModel> model,
            bool isSubProcess = false);
    }

    public class ContatoriService : ServiceBase, IContatoriService
    {
        private readonly IPar_GiustificativiRepository                      _par_GiustificativiRepository;
        private readonly IDip_Rapporto_Giustificativi_MaturazioneRepository _maturazioneRepository;
        private readonly IDip_Contatori_RiportoRepository                   _riportoRepository;
        private readonly IDip_GG_GiustificativiRepository                   _giustificativiRepository;

        public ContatoriService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IPar_GiustificativiRepository par_GiustificativiRepository,
            IDip_Rapporto_Giustificativi_MaturazioneRepository maturazioneRepository,
            IDip_Contatori_RiportoRepository riportoRepository,
            IDip_GG_GiustificativiRepository giustificativiRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _maturazioneRepository        = maturazioneRepository;
            _riportoRepository            = riportoRepository;
            _giustificativiRepository     = giustificativiRepository;
        }

        public virtual async Task<GenericResult<Contatori_Calcolo_OutModel>> CalcolaContatori(
            GenericRequest<Contatori_Calcolo_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Contatori_Calcolo_OutModel();

                int idRapporto = model.Data.IdDip_RapportoLavoro;
                int anno       = model.Data.Anno;
                int mese       = model.Data.Mese; // 1-12

                // ?? 1. Giustificativi marcati come contatore ??????????????????????
                var tuttiJust = _par_GiustificativiRepository
                    .FindAll(j => j.TipoContatore != TipoContatore.NoContatore)
                    .ToList();

                if (!tuttiJust.Any())
                    return retVal;

                // ?? 2. Piani di maturazione per questo rapporto ???????????????????
                var idJust = tuttiJust.Select(j => j.Id).ToHashSet();

                var pianiDb = _maturazioneRepository
                    .FindAll(m => m.IdDip_RapportoLavoro == idRapporto && idJust.Contains(m.IdPar_Giustificativi))
                    .ToList();

                // ?? 3. Riporto mese 0 per l'anno corrente ????????????????????????
                var riportiDb = _riportoRepository
                    .FindAll(r => r.IdDip_RapportoLavoro == idRapporto &&
                                  r.Anno                 == anno       &&
                                  idJust.Contains(r.IdPar_Giustificativi))
                    .ToList();

                // ?? Override runtime: sostituisce DB con i valori non ancora salvati ????
                var riportiOverride    = model.Data.RiportiOverride;
                var maturazioneOverride = model.Data.MaturazioneOverride;

                // ?? 4. Tutti i giustificativi dell'anno per il rapporto ???????????
                var giustAnno = _giustificativiRepository
                    .FindAll(g => g.IdDip_RapportoLavoro == idRapporto &&
                                  g.Data.Year            == anno       &&
                                  idJust.Contains(g.IdPar_Giustificativi))
                    .ToList();

                // ?? 5. Calcolo per ciascun giustificativo ?????????????????????????
                foreach (var just in tuttiJust)
                {
                    // Maturazione: override ha precedenza sul DB
                    TimeSpan oreMensili;
                    if (maturazioneOverride != null)
                    {
                        var ovMat = maturazioneOverride.FirstOrDefault(m => m.IdPar_Giustificativi == just.Id);
                        oreMensili = ovMat != null
                            ? ParseTimeSpan(ovMat.OreMaturazione)
                            : (pianiDb.FirstOrDefault(p => p.IdPar_Giustificativi == just.Id)?.OreMaturazione ?? TimeSpan.Zero);
                    }
                    else
                    {
                        oreMensili = pianiDb.FirstOrDefault(p => p.IdPar_Giustificativi == just.Id)?.OreMaturazione ?? TimeSpan.Zero;
                    }

                    // Riporto: override ha precedenza sul DB
                    TimeSpan saldoRiporto;
                    if (riportiOverride != null)
                    {
                        var ovRip = riportiOverride.FirstOrDefault(r => r.IdPar_Giustificativi == just.Id);
                        saldoRiporto = ovRip != null
                            ? ParseTimeSpan(ovRip.SaldoRiporto)
                            : (riportiDb.FirstOrDefault(r => r.IdPar_Giustificativi == just.Id)?.SaldoRiporto ?? TimeSpan.Zero);
                    }
                    else
                    {
                        saldoRiporto = riportiDb.FirstOrDefault(r => r.IdPar_Giustificativi == just.Id)?.SaldoRiporto ?? TimeSpan.Zero;
                    }

                    // Giustificativi goduti suddivisi per periodo
                    var giustJust = giustAnno.Where(g => g.IdPar_Giustificativi == just.Id).ToList();

                    TimeSpan godutoPrecedente  = SommaOre(giustJust.Where(g => g.Data.Month < mese));
                    TimeSpan godutoCorrente    = SommaOre(giustJust.Where(g => g.Data.Month == mese));
                    TimeSpan godutoSuccessivo  = SommaOre(giustJust.Where(g => g.Data.Month > mese));

                    // Maturato per periodo
                    int mesiPrecedenti  = mese - 1;                 // 0 se mese=1
                    int mesiSuccessivi  = 12 - mese;                // 0 se mese=12

                    TimeSpan maturatoPrecedente = saldoRiporto + Moltiplica(oreMensili, mesiPrecedenti);
                    TimeSpan maturatoCorrente   = oreMensili;
                    TimeSpan maturatoSuccessivo = Moltiplica(oreMensili, mesiSuccessivi);

                    retVal.Risultati.Add(new Contatori_Giustificativo_Result
                    {
                        IdPar_Giustificativi = just.Id,
                        Descrizione          = just.Descrizione ?? string.Empty,
                        Codice               = just.Codice      ?? string.Empty,

                        PeriodoPrecedente = new Contatori_Periodo
                        {
                            Maturato = TimeSpanToStringDisplay(maturatoPrecedente),
                            Goduto   = TimeSpanToStringDisplay(godutoPrecedente),
                            Saldo    = TimeSpanToStringDisplay(maturatoPrecedente - godutoPrecedente)
                        },
                        PeriodoCorrente = new Contatori_Periodo
                        {
                            Maturato = TimeSpanToStringDisplay(maturatoCorrente),
                            Goduto   = TimeSpanToStringDisplay(godutoCorrente),
                            Saldo    = TimeSpanToStringDisplay(maturatoCorrente - godutoCorrente)
                        },
                        PeriodoSuccessivo = new Contatori_Periodo
                        {
                            Maturato = TimeSpanToStringDisplay(maturatoSuccessivo),
                            Goduto   = TimeSpanToStringDisplay(godutoSuccessivo),
                            Saldo    = TimeSpanToStringDisplay(maturatoSuccessivo - godutoSuccessivo)
                        }
                    });
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }

        // ?? Helpers ??????????????????????????????????????????????????????????

        /// <summary>Somma le ore (Hours) di una lista di giustificativi.</summary>
        private static TimeSpan SommaOre(IEnumerable<Dip_GG_Giustificativi> lista)
            => lista.Aggregate(TimeSpan.Zero, (acc, g) => acc + (g.Hours ?? TimeSpan.Zero));

        /// <summary>Moltiplica un TimeSpan per un intero.</summary>
        private static TimeSpan Moltiplica(TimeSpan ts, int fattore)
            => fattore <= 0 ? TimeSpan.Zero : TimeSpan.FromTicks(ts.Ticks * fattore);

        private static Contatori_Riporto_Model MapRiporto(Dip_Contatori_Riporto e) => new()
        {
            Id                   = e.Id,
            IdDip_RapportoLavoro = e.IdDip_RapportoLavoro,
            IdPar_Giustificativi = e.IdPar_Giustificativi,
            Anno                 = e.Anno,
            SaldoRiporto         = TimeSpanToString(e.SaldoRiporto),
            IsManuale            = e.IsManuale
        };

        /// <summary>
        /// Formato riporto editabile: "HHH:MM" — ore totali e minuti, senza secondi.
        /// </summary>
        private static string TimeSpanToString(TimeSpan ts)
            => $"{(long)ts.TotalHours:D2}:{ts.Minutes:D2}";

        /// <summary>
        /// Formato visualizzazione contatori: "Xg HH:mm" se giorni > 0, altrimenti "HH:mm".
        /// </summary>
        private static string TimeSpanToStringDisplay(TimeSpan ts)
        {
            int days  = (int)ts.TotalDays;
            int hours = ts.Hours;
            int mins  = ts.Minutes;
            return days > 0
                ? $"{days}g {hours:D2}:{mins:D2}"
                : $"{hours:D2}:{mins:D2}";
        }

        /// <summary>
        /// Deserializza "HHH:MM" o "HHH:MM:SS" in TimeSpan.
        /// NON usa TimeSpan.Parse che interpreta il primo segmento come giorni
        /// e quindi fallisce per ore >= 24 (es. "30:00" verrebbe letto come 30 giorni).
        /// </summary>
        private static TimeSpan ParseTimeSpan(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return TimeSpan.Zero;
            var parts = s.Trim().Split(':');
            if (parts.Length >= 2
                && long.TryParse(parts[0].Trim(), out long totalHours)
                && int.TryParse(parts[1].Trim(), out int minutes))
            {
                int seconds = parts.Length >= 3 && int.TryParse(parts[2].Trim(), out int sec) ? sec : 0;
                return TimeSpan.FromSeconds(totalHours * 3600L + minutes * 60 + seconds);
            }
            return TimeSpan.Zero;
        }

        // ?? Riporto (Mese 0) ?????????????????????????????????????????????????

        public virtual async Task<GenericResult<Contatori_Riporto_GetAll_OutModel>> Riporto_GetAll(
            GenericRequest<Contatori_Riporto_GetAll_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Contatori_Riporto_GetAll_OutModel();

                var lista = _riportoRepository
                    .FindAll(r => r.IdDip_RapportoLavoro == model.Data.IdDip_RapportoLavoro
                               && r.Anno                 == model.Data.Anno)
                    .ToList();

                retVal.Riporti = lista.Select(MapRiporto).ToList();

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Contatori_Riporto_Upsert_OutModel>> Riporto_Upsert(
            GenericRequest<Contatori_Riporto_Upsert_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Contatori_Riporto_Upsert_OutModel();

                var src = model.Data.Riporto;

                // Cerca prima per Id, poi per chiave business (evita duplicati se Id=0 o stale)
                Dip_Contatori_Riporto? entity = null;
                if (src.Id > 0)
                    entity = await _riportoRepository.FindByIdAsync(src.Id);

                if (entity == null)
                    entity = _riportoRepository.FindAll(r =>
                        r.IdDip_RapportoLavoro == src.IdDip_RapportoLavoro &&
                        r.IdPar_Giustificativi == src.IdPar_Giustificativi &&
                        r.Anno                 == src.Anno).FirstOrDefault();

                if (entity == null)
                    entity = new Dip_Contatori_Riporto
                    {
                        IdDip_RapportoLavoro = src.IdDip_RapportoLavoro,
                        IdPar_Giustificativi = src.IdPar_Giustificativi,
                        Anno                 = src.Anno
                    };

                entity.SaldoRiporto = ParseTimeSpan(src.SaldoRiporto);
                entity.IsManuale    = true;

                var saved = await _riportoRepository.UpsertAsync(entity);
                retVal.Riporto = MapRiporto(saved);

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Contatori_Riporto_Delete_OutModel>> Riporto_Delete(
            GenericRequest<Contatori_Riporto_Delete_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _riportoRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _riportoRepository.DeleteAsync(entity);

                await Task.Delay(DelayAsyncMethod);
                return new Contatori_Riporto_Delete_OutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Contatori_Anno_OutModel>> CalcolaContatori_Anno(
            GenericRequest<Contatori_Anno_InModel> model,
            bool isSubProcess = false)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Contatori_Anno_OutModel();

                for (int mese = 1; mese <= 12; mese++)
                {
                    var reqMese = new GenericRequest<Contatori_Calcolo_InModel>();
                    reqMese.Data.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;
                    reqMese.Data.Anno                 = model.Data.Anno;
                    reqMese.Data.Mese                 = mese;
                    // propaga gli override runtime (possono essere null: nessun problema)
                    reqMese.Data.RiportiOverride      = model.Data.RiportiOverride;
                    reqMese.Data.MaturazioneOverride   = model.Data.MaturazioneOverride;

                    var resMese = await CalcolaContatori(reqMese, true);

                    retVal.Mesi.Add(new Contatori_Anno_MeseResult
                    {
                        Mese      = mese,
                        Risultati = resMese.Success && resMese.Data != null
                                        ? resMese.Data.Risultati
                                        : new List<Contatori_Giustificativo_Result>()
                    });
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }
}
