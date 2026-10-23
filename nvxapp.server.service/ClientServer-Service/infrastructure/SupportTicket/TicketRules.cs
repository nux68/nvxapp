using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Regole dei ticket che non dipendono dal database: livelli dei ruoli, livello superiore abilitato, allegati.
    */
    public static class TicketRules
    {
        // allegati: per file, per passaggio e totale (il base64 aumenta il peso della richiesta di circa un terzo)
        public const int AttachMaxCount = 5;
        public const long AttachMaxSize = 10L * 1024 * 1024;
        public const long AttachMaxTotalSize = 20L * 1024 * 1024;
        public static readonly string[] AttachExtensions =
        {
            ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".txt", ".log", ".csv", ".json", ".xml",
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods", ".zip", ".7z", ".eml", ".msg"
        };

        public const int OggettoMaxLength = 200;
        public const int TextMaxLength = 4000;

        // livello del ticket a cui appartiene un ruolo
        public static TicketLevel LevelOfRole(RoleCode role) => role switch
        {
            RoleCode.SuperUser => TicketLevel.SuperUser,
            RoleCode.PowerAdmin or RoleCode.Admin => TicketLevel.Dev,
            RoleCode.DealerPowerAdmin or RoleCode.DealerAdmin => TicketLevel.Dealer,
            RoleCode.FinancialAdvisorPowerAdmin or RoleCode.FinancialAdvisorAdmin => TicketLevel.FinancialAdvisor,
            RoleCode.CompanyPowerAdmin or RoleCode.CompanyAdmin => TicketLevel.Company,
            _ => TicketLevel.User,
        };

        // ruolo "Power" del livello: riceve i ticket non assegnati e li assegna
        public static RoleCode PowerRole(TicketLevel level) => level switch
        {
            TicketLevel.SuperUser => RoleCode.SuperUser,
            TicketLevel.Dev => RoleCode.PowerAdmin,
            TicketLevel.Dealer => RoleCode.DealerPowerAdmin,
            TicketLevel.FinancialAdvisor => RoleCode.FinancialAdvisorPowerAdmin,
            TicketLevel.Company => RoleCode.CompanyPowerAdmin,
            _ => RoleCode.User,
        };

        // ruolo "Admin" del livello: gestisce solo i ticket assegnati (SuperUser e User non hanno Admin)
        public static RoleCode? AdminRole(TicketLevel level) => level switch
        {
            TicketLevel.Dev => RoleCode.Admin,
            TicketLevel.Dealer => RoleCode.DealerAdmin,
            TicketLevel.FinancialAdvisor => RoleCode.FinancialAdvisorAdmin,
            TicketLevel.Company => RoleCode.CompanyAdmin,
            _ => null,
        };

        // primo livello abilitato sopra quello indicato (i livelli sopra hanno valore piu' basso); null se non ce ne sono
        public static TicketLevel? NextLevelUp(IEnumerable<TicketLevel> enabledLevels, TicketLevel from)
        {
            var sopra = enabledLevels.Where(l => l < from).ToList();
            return sopra.Count == 0 ? null : sopra.Max();
        }

        // errore (gia' tradotto) se gli allegati non rispettano i limiti, altrimenti null
        public static string? AttachError(List<TicketAttachModel>? attach)
        {
            if (attach == null || attach.Count == 0)
                return null;

            if (attach.Count > AttachMaxCount)
                return Localization("TicketAttachTooMany", AttachMaxCount);

            long totale = 0;
            foreach (var a in attach)
            {
                long size = a.File?.LongLength ?? 0;
                if (size == 0)
                    return Localization("TicketAttachEmpty", a.FileName);
                if (size > AttachMaxSize)
                    return Localization("TicketAttachTooLarge", a.FileName, AttachMaxSize / (1024 * 1024));

                string ext = Path.GetExtension(a.FileName ?? string.Empty).ToLowerInvariant();
                if (!AttachExtensions.Contains(ext))
                    return Localization("TicketAttachExtensionNotAllowed", a.FileName ?? string.Empty);

                totale += size;
            }

            if (totale > AttachMaxTotalSize)
                return Localization("TicketAttachTotalTooLarge", AttachMaxTotalSize / (1024 * 1024));

            return null;
        }

        private static string Localization(string key, params object?[] args)
            => nvxapp.server.data.Localization.InfrastructureMessages.Get(key, args);
    }
}
