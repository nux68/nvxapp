using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    // Causali e giustificativi a importo (TipoValore.Importo) non possono stare dove il motore mette delle ore:
    // causali di profili, orari e fasce orarie, giustificativo di assenza ingiustificata del profilo
    public static class GestionePresenzeTipoValore
    {
        /// <summary>Errore se una delle causali indicate (0/null = nessuna) e' a importo.</summary>
        public static void ControllaCausaliOrarie(IPar_CausaliRepository repository, params int?[] idCausali)
        {
            var ids = idCausali.Where(x => x is > 0).Select(x => x!.Value).Distinct().ToList();
            if (ids.Count == 0)
                return;
            var aImporto = repository.FindAll(x => ids.Contains(x.Id) && x.Tipo == TipoValore.Importo).FirstOrDefault();
            if (aImporto != null)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ParCausaleImportoNonAmmessa", aImporto.Codice));
        }

        /// <summary>Errore se il giustificativo indicato e' a importo.</summary>
        public static void ControllaGiustificativoOrario(IPar_GiustificativiRepository repository, int idGiustificativo)
        {
            if (idGiustificativo <= 0)
                return;
            var aImporto = repository.FindAll(x => x.Id == idGiustificativo && x.Tipo == TipoValore.Importo).FirstOrDefault();
            if (aImporto != null)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ParGiustificativoImportoNonAmmesso", aImporto.Codice));
        }
    }
}
