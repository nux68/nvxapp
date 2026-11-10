using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed
{
    /*
     Crea all'avvio i listini iniziali dichiarati dai rami (ILicenseCatalogSeed), ANALISI_LICENZE.md §15.5.
     Ogni listino (tipo soggetto + funzione) si crea UNA sola volta, segnata in AppSetting
     (Licenze_ListinoIniziale_<TipoSoggetto>_<Ambito>):
        - se esiste gia' un listino globale per quel tipo e quella funzione (attivo o no), non si tocca e si segna come fatto
        - se il SuperUser lo modifica o lo elimina, ai riavvii non viene ricreato
        - un ramo aggiunto dopo (nuovo applicativo) crea il suo listino al primo avvio
    */
    public interface ILicenseCatalogSeeder
    {
        /// <summary>Crea i listini iniziali mancanti; restituisce quanti ne ha creati.</summary>
        Task<int> SeedAsync();
    }

    public class LicenseCatalogSeeder : ILicenseCatalogSeeder
    {
        private readonly IEnumerable<ILicenseCatalogSeed> _seeds;
        private readonly ILic_ListinoRepository _listinoRepository;
        private readonly IAppSettingRepository _appSettingRepository;

        public LicenseCatalogSeeder(IEnumerable<ILicenseCatalogSeed> seeds,
                                    ILic_ListinoRepository listinoRepository,
                                    IAppSettingRepository appSettingRepository)
        {
            _seeds = seeds;
            _listinoRepository = listinoRepository;
            _appSettingRepository = appSettingRepository;
        }

        public static string ChiaveFatto(TipoSoggettoLicenza tipo, AmbitoLicenza ambito) => $"Licenze_ListinoIniziale_{tipo}_{ambito}";

        public async Task<int> SeedAsync()
        {
            int creati = 0;
            foreach (var seed in _seeds)
            {
                foreach (var listino in seed.Listini())
                {
                    var chiave = ChiaveFatto(listino.TipoSoggetto, listino.Ambito);
                    if (!string.IsNullOrEmpty(await _appSettingRepository.GetValueAsync(chiave)))
                        continue;

                    var tipo = listino.TipoSoggetto;
                    var ambito = listino.Ambito;
                    bool esiste = await _listinoRepository.FindAll(x => x.IdDealer == null && x.TipoSoggetto == tipo && x.Ambito == ambito).AnyAsync();
                    if (!esiste)
                    {
                        await _listinoRepository.CreateAsync(listino);
                        creati++;
                        Log.Information("Licenze: creato il listino iniziale {Descrizione} ({Seed}: {Tipo}, {Ambito})", listino.Descrizione, seed.Name, tipo, ambito);
                    }
                    await _appSettingRepository.SetValueAsync(chiave, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            return creati;
        }
    }
}
