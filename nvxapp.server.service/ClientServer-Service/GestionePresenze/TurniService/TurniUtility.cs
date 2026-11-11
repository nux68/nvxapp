using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService
{
    /*
     Funzioni comuni ai servizi dei turni (AT-TURNI): azienda corrente, ruoli, testo dell'orario del turno,
     dipendenti di un reparto (Az_SediRepartoUser.UserInDepartment, come le approvazioni) con il rapporto di lavoro.
    */
    public class TurniUtility : ITurniUtility
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IAz_SediCommonRepository _az_SediRepository;
        private readonly IAz_SediRepartoCommonRepository _az_SediRepartoRepository;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;

        public TurniUtility(UserManager<ApplicationUser> userManager,
                            IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                            IPar_OrarioRepository par_OrarioRepository,
                            IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository,
                            IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                            IAz_SediCommonRepository az_SediRepository,
                            IAz_SediRepartoCommonRepository az_SediRepartoRepository,
                            IDip_AnagraficaRepository dip_AnagraficaRepository,
                            IDip_RapportoLavoroRepository dip_RapportoLavoroRepository)
        {
            _userManager = userManager;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;
            _az_SediRepository = az_SediRepository;
            _az_SediRepartoRepository = az_SediRepartoRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
        }

        public async Task<int> IdAziendaAsync(string currentCompany)
        {
            if (!int.TryParse(currentCompany, out int idCompany) || idCompany <= 0)
                return 0;
            var company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
            return company_DATA?.az_Anagrafica?.Id ?? 0;
        }

        public async Task<bool> HaRuoloAsync(string idAspNetUsers, params string[] ruoli)
        {
            var user = await _userManager.FindByIdAsync(idAspNetUsers);
            if (user == null)
                return false;
            foreach (var ruolo in ruoli)
                if (await _userManager.IsInRoleAsync(user, ruolo))
                    return true;
            return false;
        }

        public Task<bool> IsAdminAsync(string idAspNetUsers) => HaRuoloAsync(idAspNetUsers, "CompanyPowerAdmin", "CompanyAdmin");

        // orario del turno come testo ("06:00-14:00", "08:00-12:00 / 13:00-17:00", "6:00 h") e minuti teorici
        public (string Testo, int Minuti) OrarioTesto(int? idPar_Orario)
        {
            if (idPar_Orario == null)
                return (string.Empty, 0);
            var orario = _par_OrarioRepository.FindAll(o => o.Id == idPar_Orario.Value).FirstOrDefault();
            if (orario == null)
                return (string.Empty, 0);
            if (orario.TimbratureTipo != OrarioTimbratureTipo.IntervalloOrario)
                return (orario.Hh_Teo_MonteOre.ToString("H:mm") + " h", (int)orario.Hh_Teo_MonteOre.ToTimeSpan().TotalMinutes);

            var coppie = _par_OrarioIntervalloHHRepository.FindAll(c => c.IdPar_Orario == orario.Id && c.Dalle != null && c.Alle != null)
                                                          .OrderBy(c => c.NumCoppia).ToList();
            int minuti = 0;
            foreach (var c in coppie)
            {
                var durata = c.Alle!.Value.ToTimeSpan() - c.Dalle!.Value.ToTimeSpan();
                if (durata < TimeSpan.Zero) durata += TimeSpan.FromDays(1);   // turno a cavallo della mezzanotte
                minuti += (int)durata.TotalMinutes;
            }
            return (string.Join(" / ", coppie.Select(c => $"{c.Dalle!.Value:HH\\:mm}-{c.Alle!.Value:HH\\:mm}")), minuti);
        }

        public Par_TurnoModel Modello(Par_Turno turno)
        {
            var (testo, minuti) = OrarioTesto(turno.IdPar_Orario);
            return new Par_TurnoModel
            {
                Id = turno.Id,
                IdAz_Anagrafica = turno.IdAz_Anagrafica,
                Codice = turno.Codice,
                Descrizione = turno.Descrizione,
                IdPar_Orario = turno.IdPar_Orario,
                Colore = turno.Colore,
                OrarioTesto = testo,
                MinutiTeorici = minuti
            };
        }

        // reparto dell'azienda (null se non esiste o e' di un'altra azienda)
        public Az_SediReparto? Reparto(int idAz_SediReparto, int idAzienda)
        {
            var reparto = _az_SediRepartoRepository.FindAll(r => r.Id == idAz_SediReparto).FirstOrDefault();
            if (reparto == null)
                return null;
            return _az_SediRepository.FindAll(s => s.Id == reparto.IdAz_Sedi && s.IdAz_Anagrafica == idAzienda).Any() ? reparto : null;
        }

        public static string Nominativo(Dip_Anagrafica a) => $"{a.Cognome} {a.Nome}".Trim();

        // dipendenti del reparto (UserInDepartment) con il rapporto di lavoro attivo nel periodo
        public List<TurnoDipendenteModel> DipendentiReparto(int idAz_SediReparto, int idAzienda, DateTime dal, DateTime al)
        {
            var utenti = _az_SediRepartoUserRepository.FindAll(x => x.IdAz_SediReparto == idAz_SediReparto && x.UserInDepartment)
                                                      .Select(x => x.IdAspNetUsers).Distinct().ToList();
            return Dipendenti(utenti, idAzienda, dal, al);
        }

        public List<TurnoDipendenteModel> Dipendenti(List<string> idAspNetUsers, int idAzienda, DateTime dal, DateTime al)
        {
            var anagrafiche = _dip_AnagraficaRepository.FindAll(a => a.IdAz_Anagrafica == idAzienda && a.IdAspNetUsers != null && idAspNetUsers.Contains(a.IdAspNetUsers))
                                                       .ToList();
            var idAnagrafiche = anagrafiche.Select(a => a.Id).ToList();
            var rapporti = _dip_RapportoLavoroRepository.FindAll(r => idAnagrafiche.Contains(r.IdDip_Anagrafica)).ToList();

            var retVal = new List<TurnoDipendenteModel>();
            foreach (var a in anagrafiche)
            {
                var rapporto = rapporti.Where(r => r.IdDip_Anagrafica == a.Id &&
                                                   (r.DataAss == null || r.DataAss.Value.Date <= al.Date) &&
                                                   (r.DataLic == null || r.DataLic.Value.Date >= dal.Date))
                                       .OrderByDescending(r => r.DataAss).FirstOrDefault();
                if (rapporto == null)
                    continue;
                retVal.Add(new TurnoDipendenteModel
                {
                    IdAspNetUsers = a.IdAspNetUsers!,
                    IdDip_Anagrafica = a.Id,
                    IdDip_RapportoLavoro = rapporto.Id,
                    Nominativo = Nominativo(a)
                });
            }
            return retVal.OrderBy(d => d.Nominativo).ToList();
        }

        // reparti in cui l'utente lavora (UserInDepartment)
        public List<int> RepartiUtente(string idAspNetUsers)
            => _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.UserInDepartment)
                                            .Select(x => x.IdAz_SediReparto).Distinct().ToList();

        public string RepartoDescrizione(int? idAz_SediReparto)
        {
            if (idAz_SediReparto == null)
                return string.Empty;
            return _az_SediRepartoRepository.FindAll(r => r.Id == idAz_SediReparto.Value).Select(r => r.Descrizione).FirstOrDefault() ?? string.Empty;
        }

        // dipendente (anagrafica e utente) del rapporto di lavoro
        public (Dip_Anagrafica? Anagrafica, Dip_RapportoLavoro? Rapporto) DipendenteDelRapporto(int idDip_RapportoLavoro)
        {
            var rapporto = _dip_RapportoLavoroRepository.FindAll(r => r.Id == idDip_RapportoLavoro).FirstOrDefault();
            if (rapporto == null)
                return (null, null);
            return (_dip_AnagraficaRepository.FindAll(a => a.Id == rapporto.IdDip_Anagrafica).FirstOrDefault(), rapporto);
        }
    }

    public interface ITurniUtility
    {
        Task<int> IdAziendaAsync(string currentCompany);
        Task<bool> HaRuoloAsync(string idAspNetUsers, params string[] ruoli);
        Task<bool> IsAdminAsync(string idAspNetUsers);
        (string Testo, int Minuti) OrarioTesto(int? idPar_Orario);
        Par_TurnoModel Modello(Par_Turno turno);
        Az_SediReparto? Reparto(int idAz_SediReparto, int idAzienda);
        List<TurnoDipendenteModel> DipendentiReparto(int idAz_SediReparto, int idAzienda, DateTime dal, DateTime al);
        List<TurnoDipendenteModel> Dipendenti(List<string> idAspNetUsers, int idAzienda, DateTime dal, DateTime al);
        List<int> RepartiUtente(string idAspNetUsers);
        string RepartoDescrizione(int? idAz_SediReparto);
        (Dip_Anagrafica? Anagrafica, Dip_RapportoLavoro? Rapporto) DipendenteDelRapporto(int idDip_RapportoLavoro);
    }
}
