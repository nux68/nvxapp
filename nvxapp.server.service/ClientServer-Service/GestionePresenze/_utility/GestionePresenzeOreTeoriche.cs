using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // Ore teoriche giorno per giorno di un rapporto di lavoro, lette dal profilo orario attivo.
    // Stessa regola del motore di calcolo (TimeSheet_EngineService.CalcolaOreTeoriche):
    // vale l'orario con ZOrder piu' basso del giorno; a coppie somma gli intervalli, a monte ore prende Hh_Teo_MonteOre.
    // Un orario senza coppie (es. RIPOSO) vale 0 ore: giorno non lavorativo.
    public class GestionePresenzeOreTeoriche : IGestionePresenzeOreTeoriche
    {
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;

        public GestionePresenzeOreTeoriche(IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                           IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                           IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository,
                                           IPar_OrarioRepository par_OrarioRepository,
                                           IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository)
        {
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
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

            var idOrari = parProfiliGG.Select(g => g.IdPar_Orario).Distinct().ToList();
            var orari = _par_OrarioRepository.FindAll(o => idOrari.Contains(o.Id)).ToList();
            var coppie = _par_OrarioIntervalloHHRepository.FindAll(c => idOrari.Contains(c.IdPar_Orario)).ToList();

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

                var ore = TimeSpan.Zero;
                if (orario?.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                {
                    foreach (var coppia in coppie.Where(c => c.IdPar_Orario == orario.Id && c.Dalle.HasValue && c.Alle.HasValue))
                    {
                        var durata = coppia.Alle!.Value.ToTimeSpan() - coppia.Dalle!.Value.ToTimeSpan();
                        if (durata > TimeSpan.Zero)
                            ore += durata;
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
    }

    public interface IGestionePresenzeOreTeoriche
    {
        Dictionary<DateTime, TimeSpan?> GetOreTeoriche(int IdDip_RapportoLavoro, DateTime Dal, DateTime Al);
    }
}
