using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    /*
     Categorie di notifica di Compliance (INF-SVC.7): titolo dai testi del modulo, visibili nelle preferenze solo con
     Compliance attivo. Le scadenze (formazione, visite, nomine, documenti, termine delle 72 ore) restano nella categoria
     "scadenze" dello scadenzario, le prese visione (DPI, nomine) in "presevisione".
    */
    public class CmpNotificationCategoryProvider : INotificationCategoryProvider
    {
        /// <summary>avvisi del registro GDPR agli amministratori (nuova violazione dei dati): email predefinita, termine di 72 ore</summary>
        public static readonly NotificationCategory Gdpr = new()
        {
            Code = "cmp.gdpr", Application = ApplicationType.Compliance, DefaultEmail = true, Ordine = 600,
            TitoloText = () => ComplianceMessages.Get("NotificationCategory_Gdpr")
        };

        public IEnumerable<NotificationCategory> Categories => new[] { Gdpr };
    }
}
