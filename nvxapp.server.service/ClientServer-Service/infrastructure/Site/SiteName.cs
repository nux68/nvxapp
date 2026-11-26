namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Site
{
    /*
     Nome del sito lato server (es. nelle email): fare sempre riferimento a SiteName.Value.
     Come SITE_NAME del client (site-name.ts), con la stessa precedenza: applicativo completo (SiteNameAllFunction),
     poi i rami applicativi (SiteNameAttendanceTracking, SiteNameCrm, SiteNameDocumentManagement),
     altrimenti quello di Infrastructure (SiteNameInfrastructure).
    */
    public static class SiteName
    {
        public static string Value => SiteNameAllFunction.Value ?? SiteNameAttendanceTracking.Value ?? SiteNameCrm.Value
                                      ?? SiteNameDocumentManagement.Value ?? SiteNameInfrastructure.Value;
    }
}
