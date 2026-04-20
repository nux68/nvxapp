namespace nvxapp.server.service.Helpers
{
    public static class NVXSystem
    {

        public static string DownloadURL { get; } = "Account/Download/";

        public static string ExportFolder
        {
            get
            {
                string exportsFolder = Path.Combine(Path.GetTempPath(), "nvxapp_exports");
                Directory.CreateDirectory(exportsFolder);
                return exportsFolder;
            }
        }



    }

}
