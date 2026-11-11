using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // Ore teoriche giorno per giorno di un rapporto di lavoro, lette dal profilo orario attivo.
    // Stessa regola del motore di calcolo (TimeSheet_EngineService.CalcolaOreTeoriche):
    // vale l'orario con ZOrder piu' basso del giorno; a coppie somma gli intervalli, a monte ore prende Hh_Teo_MonteOre.
    // Un orario senza coppie (es. RIPOSO) vale 0 ore: giorno non lavorativo.
    // La pausa pranzo varia il teorico come nel motore (GestionePresenzePause.VariazioneTeorico).
    // Nelle festivita' del calendario aziendale vale l'orario festivo del profilo.
    public class GestionePresenzeOreTeoriche : IGestionePresenzeOreTeoriche
    {
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IPar_FestivitaRepository _par_FestivitaRepository;
        private readonly IPar_OrarioPausaRepository _par_OrarioPausaRepository;

        public GestionePresenzeOreTeoriche(IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                           IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                           IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository,
                                           IPar_OrarioRepository par_OrarioRepository,
                                           IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository,
                                           IPar_FestivitaRepository par_FestivitaRepository,
                                           IPar_OrarioPausaRepository par_OrarioPausaRepository)
        {
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_FestivitaRepository = par_FestivitaRepository;
            _par_OrarioPausaRepository = par_OrarioPausaRepository;
        }

        // Una voce per ogni giorno da Dal ad Al: null se quel giorno non e' coperto da un profilo orario
        public Dictionary<DateTime, TimeSpan?> GetOreTeoriche(int IdDip_RapportoLavoro, DateTime Dal, DateTime Al)
        {
            var retVal = new Dictionary<DateTime, TimeSpan?>();

            var profili = _dip_ProfiloOrarioRepository.FindAll(p => p.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                    p.IdPar_ProfiloOrario != null &&
                                                                    p.Dal <= Al.Date &&
                                                                    p.Al >= Dal.Date)
                                                      .ToList();

            var idProfili = profili.Select(p => p.IdPar_ProfiloOrario!.Value).Distinct().ToList();
            var parProfili = _par_ProfiloOrarioRepository.FindAll(p => idProfili.Contains(p.Id)).ToList();
            var parProfiliGG = _par_ProfiloOrarioGGRepository.FindAll(g => idProfili.Contains(g.IdPar_ProfiloOrario)).ToList();

            var idOrari = parProfiliGG.Select(g => g.IdPar_Orario)
                                      .Concat(parProfili.Select(p => p.IdPar_Orario_Festivo))
                                      .Distinct().ToList();

            // calendario festivita' dell'azienda (nei festivi vale l'orario festivo del profilo, come nel motore)
            var idAziende = parProfili.Select(p => p.IdAz_Anagrafica).Distinct().ToList();
            var festivita = _par_FestivitaRepository.FindAll(f => idAziende.Contains(f.IdAz_Anagrafica))
                                                    .ToList()
                                                    .Select(f => new Par_FestivitaModel { TipoFestivita = f.TipoFestivita, Data = f.Data })
                                                    .ToList();
            var orari = _par_OrarioRepository.FindAll(o => idOrari.Contains(o.Id)).ToList();
            var coppie = _par_OrarioIntervalloHHRepository.FindAll(c => idOrari.Contains(c.IdPar_Orario)).ToList();
            var pause = _par_OrarioPausaRepository.FindAll(p => idOrari.Contains(p.IdPar_Orario)).ToList();

            for (var giorno = Dal.Date; giorno <= Al.Date; giorno = giorno.AddDays(1))
            {
                retVal[giorno] = null;

                var profilo = profili.FirstOrDefault(p => p.Dal <= giorno && p.Al >= giorno);
                var parProfilo = profilo != null ? parProfili.FirstOrDefault(p => p.Id == profilo.IdPar_ProfiloOrario) : null;
                if (profilo == null || parProfilo == null)
                    continue;

                int numGiorno;
                if (parProfilo.TipoProfilo == TipoProfilo.Settimanale)
                    numGiorno = giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)giorno.DayOfWeek;
                else
                    numGiorno = ((giorno - profilo.Dal).Days + profilo.NumGiornoPartenzaCiclo) % parProfilo.NumGiorniCiclo;

                var orarioBase = parProfiliGG.Where(g => g.IdPar_ProfiloOrario == parProfilo.Id && g.NumGiorno == numGiorno)
                                             .OrderBy(g => g.ZOrder)
                                             .FirstOrDefault();
                var orario = orarioBase != null ? orari.FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario) : null;
                if (parProfilo.IdPar_Orario_Festivo > 0 && GestionePresenzeFestivita.IsFestivita(festivita, giorno))
                    orario = orari.FirstOrDefault(o => o.Id == parProfilo.IdPar_Orario_Festivo);

                var ore = TimeSpan.Zero;
                if (orario?.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                {
                    foreach (var coppia in coppie.Where(c => c.IdPar_Orario == orario.Id && c.Dalle.HasValue && c.Alle.HasValue))
                    {
                        var durata = coppia.Alle!.Value.ToTimeSpan() - coppia.Dalle!.Value.ToTimeSpan();
                        if (durata > TimeSpan.Zero)
                            ore += durata;
                    }

                    var pauseOrario = pause.Where(p => p.IdPar_Orario == orario.Id).Select(ModelloPausa).ToList();
                    if (pauseOrario.Count > 0)
                    {
                        var coppieOrario = coppie.Where(c => c.IdPar_Orario == orario.Id).Select(ModelloCoppia).ToList();
                        ore += GestionePresenzePause.VariazioneTeorico(giorno, ore, coppieOrario, pauseOrario);
                        if (ore < TimeSpan.Zero)
                            ore = TimeSpan.Zero;
                    }
                }
                else if (orario != null)
                {
                    ore = orario.Hh_Teo_MonteOre.ToTimeSpan();
                }

                retVal[giorno] = ore;
            }

            return retVal;
        }

        private static Par_OrarioIntervalloHHModel ModelloCoppia(Par_OrarioIntervalloHH c) => new()
        {
            NumCoppia = c.NumCoppia,
            Dalle = c.Dalle,
            Dalle_Limite_SX = c.Dalle_Limite_SX,
            Dalle_Limite_DX = c.Dalle_Limite_DX,
            Dalle_Use_4_Match = c.Dalle_Use_4_Match,
            Alle = c.Alle,
            Alle_Limite_SX = c.Alle_Limite_SX,
            Alle_Limite_DX = c.Alle_Limite_DX,
            Alle_Use_4_Match = c.Alle_Use_4_Match
        };

        private static Par_OrarioPausaModel ModelloPausa(Par_OrarioPausa p) => new()
        {
            Id = p.Id,
            IdPar_Orario = p.IdPar_Orario,
            NumPausa = p.NumPausa,
            Tipo = p.Tipo,
            Modalita = p.Modalita,
            DurataMin = p.DurataMin,
            DurataMaxMin = p.DurataMaxMin,
            SogliaMin = p.SogliaMin,
            FinestraDalle = p.FinestraDalle,
            FinestraAlle = p.FinestraAlle,
            RetribuitaMin = p.RetribuitaMin,
            IdCausale_Pausa = p.IdCausale_Pausa
        };
    }

    public interface IGestionePresenzeOreTeoriche
    {
        Dictionary<DateTime, TimeSpan?> GetOreTeoriche(int IdDip_RapportoLavoro, DateTime Dal, DateTime Al);
    }
}
