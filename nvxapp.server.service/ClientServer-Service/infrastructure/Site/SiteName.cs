namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Site
{
    /*
     Nome del sito lato server (es. nelle email): fare sempre riferimento a SiteName.Value.
     Come SITE_NAME del client (site-name.ts): vale quello del ramo applicativo se lo ridefinisce
     (SiteNameAttendanceTracking), altrimenti quello di Infrastructure (SiteNameInfrastructure).
    */
    public static class SiteName
    {
        public static string Value => SiteNameAttendanceTracking.Value ?? SiteNameInfrastructure.Value;
    }
}
