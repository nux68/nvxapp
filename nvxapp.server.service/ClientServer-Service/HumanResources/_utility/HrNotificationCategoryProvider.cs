using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Categorie di notifica di Risorse Umane (INF-SVC.7, HR-SEG.2): titolo dai testi del modulo, visibili nelle preferenze
     solo con Risorse Umane attivo. Le scadenze (formazione, checklist) restano nella categoria "scadenze" dello scadenzario,
     le prese visione in "presevisione".
    */
    public class HrNotificationCategoryProvider : INotificationCategoryProvider
    {
        /// <summary>nuovo cedolino o CU disponibile (email predefinita: documento personale da aprire)</summary>
        public static readonly NotificationCategory Cedolini = new()
        {
            Code = "hr.cedolini", Application = ApplicationType.HumanResources, DefaultEmail = true, Ordine = 500,
            TitoloText = () => HumanResourcesMessages.Get("NotificationCategory_Cedolini")
        };

        public IEnumerable<NotificationCategory> Categories => new[] { Cedolini };
    }
}
