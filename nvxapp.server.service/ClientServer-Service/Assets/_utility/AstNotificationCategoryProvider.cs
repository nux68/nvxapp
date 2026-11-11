using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Categorie di notifica di Risorse aziendali (INF-SVC.7): titolo dai testi del modulo, visibili nelle preferenze solo con
     Risorse aziendali attivo. Le scadenze (revisioni, assicurazioni, garanzie, restituzioni) restano nella categoria
     "scadenze" dello scadenzario, i verbali di consegna in "presevisione".
    */
    public class AstNotificationCategoryProvider : INotificationCategoryProvider
    {
        /// <summary>flotta: multe notificate al conducente</summary>
        public static readonly NotificationCategory Flotta = new()
        {
            Code = "ast.flotta", Application = ApplicationType.Assets, DefaultEmail = true, Ordine = 700,
            TitoloText = () => AssetsMessages.Get("NotificationCategory_Flotta")
        };

        /// <summary>beni e veicoli da restituire dai dipendenti cessati (avviso agli amministratori)</summary>
        public static readonly NotificationCategory Beni = new()
        {
            Code = "ast.beni", Application = ApplicationType.Assets, Ordine = 710,
            TitoloText = () => AssetsMessages.Get("NotificationCategory_Beni")
        };

        /// <summary>prenotazioni: annullate da un amministratore</summary>
        public static readonly NotificationCategory Prenotazioni = new()
        {
            Code = "ast.prenotazioni", Application = ApplicationType.Assets, Ordine = 720,
            TitoloText = () => AssetsMessages.Get("NotificationCategory_Prenotazioni")
        };

        public IEnumerable<NotificationCategory> Categories => new[] { Flotta, Beni, Prenotazioni };
    }
}
