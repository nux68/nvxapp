using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models
{
    public class Az_ClienteModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Az_Cliente_GetAll_InModel
    {
    }

    public class Az_Cliente_GetAll_OutModel : ModelResult
    {
        public List<Az_ClienteModel> Az_Cliente { get; set; } = new List<Az_ClienteModel>();
        public Az_Cliente_GetAll_OutModel() { }
    }

    public class Az_ClienteGetInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Az_ClienteGetOutModel : ModelResult
    {
        public Az_ClienteModel Az_Cliente { get; set; }
        public Az_ClienteGetOutModel()
        {
            Az_Cliente = new Az_ClienteModel { Id = 0, IdAz_Anagrafica = 0 };
        }
    }

    public class Az_ClientePutInModel : ModelResult
    {
        public Az_ClienteModel Az_Cliente { get; set; }
        public Az_ClientePutInModel()
        {
            Az_Cliente = new Az_ClienteModel { Id = 0, IdAz_Anagrafica = 0 };
        }
    }

    public class Az_ClientePutOutModel : ModelResult
    {
        public Az_ClienteModel Az_Cliente { get; set; }
        public Az_ClientePutOutModel()
        {
            Az_Cliente = new Az_ClienteModel { Id = 0, IdAz_Anagrafica = 0 };
        }
    }
}
