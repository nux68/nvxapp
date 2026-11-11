using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common
{
    /*
     Inizializzazione UNICA dell'anagrafica dipendente (Dip_Anagrafica, schema Common) collegata a un utente.
     E' l'unico punto che crea l'anagrafica di un utente: la usano l'inizializzatore utente comune
     (InfrastructureUserInitializer, eseguito per ogni applicativo) e gli applicativi (presenze, documentale)
     che devono garantire l'anagrafica prima di aggiungere i propri dati (es. presenze: rapporto di lavoro).

     Cognome e Nome iniziali: personaggi Disney in sequenza (modificabili poi dall'anagrafica).
     Le anagrafiche con nome provvisorio (Cognome e Nome vuoti, oppure Cognome = nome utente e Nome vuoto,
     come le creava il documentale) vengono completate con lo stesso criterio.
     I dipendenti senza utente (documentale) non passano di qui.

     Aggancio di un dipendente esistente (INF-SVC.8): creando l'account di un dipendente gia' presente in Dip_Anagrafica
     (es. Risorse Umane, Account/UserCompanyPut con IdDip_Anagrafica) l'anagrafica si collega al nuovo utente PRIMA
     degli inizializzatori, che la trovano e non ne creano una nuova. Solo anagrafiche della stessa azienda e senza utente.
    */
    public interface ICommonDipendenteUtility
    {
        /// <summary>Anagrafica dell'utente nell'azienda: creata se manca, con Cognome/Nome completati se provvisori.</summary>
        Task<Dip_Anagrafica> GetOrCreateDip_AnagraficaAsync(int idCompany, string idAspNetUsers);

        /// <summary>Garantisce l'anagrafica (creata o completata) per ciascuno degli utenti dell'azienda.</summary>
        Task AllineaDip_AnagraficaAsync(int idCompany, IEnumerable<string> idAspNetUsers);

        /// <summary>Completa Cognome/Nome provvisori delle anagrafiche esistenti degli utenti indicati.</summary>
        Task CompletaCognomeNomeAsync(int idCompany, IEnumerable<string> idAspNetUsers);

        /// <summary>true se l'anagrafica e' dell'azienda e non e' collegata a nessun utente (si puo' agganciare a un nuovo account).</summary>
        Task<bool> Dip_AnagraficaAgganciabileAsync(int idCompany, int idDip_Anagrafica);

        /// <summary>
        /// Collega l'anagrafica (dell'azienda, senza utente) all'utente indicato, che non deve averne gia' una nell'azienda.
        /// false se non e' piu' agganciabile (es. collegata nel frattempo da un'altra richiesta).
        /// </summary>
        Task<bool> AgganciaDip_AnagraficaAsync(int idCompany, int idDip_Anagrafica, string idAspNetUsers);
    }

    public class CommonDipendenteUtility : ICommonDipendenteUtility
    {
        private readonly IDip_AnagraficaCommonRepository _dip_AnagraficaRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly ICommonCompanyUtility _commonCompanyUtility;

        public CommonDipendenteUtility(IDip_AnagraficaCommonRepository dip_AnagraficaRepository,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       ICommonCompanyUtility commonCompanyUtility)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _commonCompanyUtility = commonCompanyUtility;
        }

        public async Task<Dip_Anagrafica> GetOrCreateDip_AnagraficaAsync(int idCompany, string idAspNetUsers)
        {
            var az_Anagrafica = (await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true))!;

            var dip_Anagrafica = await TrovaAsync(az_Anagrafica.Id, idAspNetUsers);
            if (dip_Anagrafica == null)
            {
                var (cognome, nome) = PersonaggioDisney(await _dip_AnagraficaRepository.FindAll(x => true).CountAsync());
                try
                {
                    dip_Anagrafica = await _dip_AnagraficaRepository.CreateAsync(new Dip_Anagrafica
                    {
                        IdAz_Anagrafica = az_Anagrafica.Id,
                        IdAspNetUsers = idAspNetUsers,
                        Cognome = cognome,
                        Nome = nome,
                        Attivo = true
                    });
                }
                catch (DbUpdateException ex)
                {
                    // creata nel frattempo da un'altra richiesta: indice unico azienda/utente
                    ex.Entries.FirstOrDefault()?.Context.ChangeTracker.Clear();
                    dip_Anagrafica = await TrovaAsync(az_Anagrafica.Id, idAspNetUsers)
                                     ?? throw new InvalidOperationException($"Dip_Anagrafica non creata per l'utente {idAspNetUsers}", ex);
                }
                return dip_Anagrafica;
            }

            await CompletaAsync(new List<Dip_Anagrafica> { dip_Anagrafica });
            return dip_Anagrafica;
        }

        public async Task AllineaDip_AnagraficaAsync(int idCompany, IEnumerable<string> idAspNetUsers)
        {
            foreach (var idAspNetUser in idAspNetUsers.Where(x => !string.IsNullOrEmpty(x)).Distinct())
                await GetOrCreateDip_AnagraficaAsync(idCompany, idAspNetUser);
        }

        public async Task CompletaCognomeNomeAsync(int idCompany, IEnumerable<string> idAspNetUsers)
        {
            var az_Anagrafica = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, false);
            if (az_Anagrafica == null)
                return;

            var utenti = idAspNetUsers.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var candidate = await _dip_AnagraficaRepository.FindAll(x => x.IdAz_Anagrafica == az_Anagrafica.Id &&
                                                                         x.IdAspNetUsers != null && utenti.Contains(x.IdAspNetUsers) &&
                                                                         (x.Nome == null || x.Nome == ""))
                                                           .OrderBy(x => x.Id)
                                                           .ToListAsync();
            await CompletaAsync(candidate);
        }

        public async Task<bool> Dip_AnagraficaAgganciabileAsync(int idCompany, int idDip_Anagrafica)
        {
            var az_Anagrafica = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, false);
            return az_Anagrafica != null &&
                   await _dip_AnagraficaRepository.FindAll(x => x.Id == idDip_Anagrafica && x.IdAz_Anagrafica == az_Anagrafica.Id && x.IdAspNetUsers == null).AnyAsync();
        }

        public async Task<bool> AgganciaDip_AnagraficaAsync(int idCompany, int idDip_Anagrafica, string idAspNetUsers)
        {
            var az_Anagrafica = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, false);
            if (az_Anagrafica == null || string.IsNullOrEmpty(idAspNetUsers) || await TrovaAsync(az_Anagrafica.Id, idAspNetUsers) != null)
                return false;

            // aggiornamento condizionato: se un'altra richiesta l'ha collegata nel frattempo non si sovrascrive
            var ora = DateTime.Now;
            int n = await _dip_AnagraficaRepository.FindAll(x => x.Id == idDip_Anagrafica && x.IdAz_Anagrafica == az_Anagrafica.Id && x.IdAspNetUsers == null)
                                                   .ExecuteUpdateAsync(u => u.SetProperty(x => x.IdAspNetUsers, idAspNetUsers)
                                                                             .SetProperty(x => x.ModifiedDate, ora));
            return n == 1;
        }

        private Task<Dip_Anagrafica?> TrovaAsync(int idAz_Anagrafica, string idAspNetUsers)
            => _dip_AnagraficaRepository.FindAll(x => x.IdAz_Anagrafica == idAz_Anagrafica && x.IdAspNetUsers == idAspNetUsers)
                                        .FirstOrDefaultAsync();

        // Completa le anagrafiche con nome provvisorio: Nome vuoto e Cognome vuoto o uguale al nome utente.
        // Il personaggio dipende dalla posizione dell'anagrafica nella tabella (come per le nuove).
        private async Task CompletaAsync(List<Dip_Anagrafica> anagrafiche)
        {
            var conNomeVuoto = anagrafiche.Where(x => string.IsNullOrWhiteSpace(x.Nome)).ToList();
            if (conNomeVuoto.Count == 0)
                return;

            var idUtenti = conNomeVuoto.Where(x => x.IdAspNetUsers != null).Select(x => x.IdAspNetUsers!).Distinct().ToList();
            var userNames = await _aspNetUsersRepository.FindAll(u => idUtenti.Contains(u.Id))
                                                        .Select(u => new { u.Id, u.UserName })
                                                        .ToDictionaryAsync(u => u.Id, u => u.UserName);

            foreach (var dip_Anagrafica in conNomeVuoto.OrderBy(x => x.Id))
            {
                var userName = dip_Anagrafica.IdAspNetUsers != null ? userNames.GetValueOrDefault(dip_Anagrafica.IdAspNetUsers) : null;
                bool provvisorio = string.IsNullOrWhiteSpace(dip_Anagrafica.Cognome) ||
                                   (userName != null && string.Equals(dip_Anagrafica.Cognome!.Trim(), userName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (!provvisorio)
                    continue;

                var (cognome, nome) = PersonaggioDisney(await _dip_AnagraficaRepository.FindAll(x => x.Id < dip_Anagrafica.Id).CountAsync());
                dip_Anagrafica.Cognome = cognome;
                dip_Anagrafica.Nome = nome;
                await _dip_AnagraficaRepository.UpsertAsync(dip_Anagrafica);
            }
        }

        // Nomi di partenza per le anagrafiche dei dipendenti: personaggi di Topolino e Paperino.
        // Finito l'elenco si riparte con un numero progressivo nel nome (es. "Topolino 2").
        private static readonly (string Cognome, string Nome)[] _personaggiDisney =
        {
            ("Mouse", "Topolino"),
            ("Mouse", "Minni"),
            ("Paperino", "Paolino"),
            ("De' Paperoni", "Paperon"),
            ("Papera", "Paperina"),
            ("Duck", "Qui"),
            ("Duck", "Quo"),
            ("Duck", "Qua"),
            ("Goof", "Pippo"),
            ("Paperone", "Gastone"),
            ("Pitagorico", "Archimede"),
            ("Papera", "Nonna"),
            ("Duck", "Paperoga"),
            ("Cavezza", "Orazio"),
            ("Cow", "Clarabella"),
            ("Gambadilegno", "Pietro"),
            ("Beta", "Eta"),
            ("Basettoni", "Commissario"),
            ("Manetta", "Ispettore"),
            ("Nera", "Macchia"),
            ("Rockerduck", "John"),
            ("McBridge", "Brigitta"),
        };

        private static (string Cognome, string Nome) PersonaggioDisney(int indice)
        {
            var p = _personaggiDisney[indice % _personaggiDisney.Length];
            int giro = indice / _personaggiDisney.Length;
            return giro == 0 ? p : (p.Cognome, $"{p.Nome} {giro + 1}");
        }
    }
}
