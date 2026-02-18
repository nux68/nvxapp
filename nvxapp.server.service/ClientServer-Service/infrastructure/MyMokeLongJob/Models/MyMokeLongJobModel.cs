using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models
{
    
    public class MyMokeLongJobModel
    {
        public string? JobParameter { get; set; }
    }


    public class MyMokeLongJobInModel
    {
    }

    public class MyMokeLongJobOutModel: ModelResult 
    {
        
        public string? JobId { get; set; }
    }
}