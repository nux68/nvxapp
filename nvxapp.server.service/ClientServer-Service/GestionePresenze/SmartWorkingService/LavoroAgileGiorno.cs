using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimbraturaEvolutaService;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService
{
    // Giorno di lavoro agile approvato (AT-SW): usato dalla timbratura evoluta per non applicare il geofence
    public class LavoroAgileGiorno : ILavoroAgileGiorno
    {
        public bool IsGiornoLavoroAgile(int idDip_RapportoLavoro, DateTime giorno) => false;
    }
}
