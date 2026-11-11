using nvxapp.server.data.Localization;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    // codici di Compliance e loro descrizioni lato server (le stesse del client sono in assets/i18n/compliance, cmp.enums.*)
    public static class CmpTesti
    {
        // tipi di corso: 0 formazione generale, 1 specifica, 2 aggiornamento, 3 antincendio, 4 primo soccorso, 5 preposti,
        // 6 dirigenti, 7 RLS, 8 RSPP/ASPP, 9 attrezzature, 10 altro
        public const int UltimoTipoCorso = 10;
        // tipi di nomina: 0 RSPP, 1 ASPP, 2 RLS, 3 preposto, 4 addetto antincendio/emergenze, 5 addetto primo soccorso,
        // 6 medico competente, 7 dirigente per la sicurezza, 8 altro
        public const int UltimoTipoNomina = 8;

        public static string TipoNomina(int tipo) => ComplianceMessages.Get("AppointmentType_" + tipo);
        public static string BaseGiuridica(int b) => ComplianceMessages.Get("LegalBasis_" + b);
        public static string TipoResponsabile(int t) => ComplianceMessages.Get("ProcessorType_" + t);
        public static string Rischio(int r) => ComplianceMessages.Get("BreachRisk_" + r);
    }
}
