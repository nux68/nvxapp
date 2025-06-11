using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models
{
    public class Az_SediModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }
    }

    public class Az_Sedi_GetAll_InModel
    {

    }

    public class Az_Sedi_GetAll_OutModel : ModelResult
    {
        public List<Az_SediModel> Az_Sedi { get; set; } = new List<Az_SediModel>();

        public Az_Sedi_GetAll_OutModel()
        {

        }
    }


    public class Az_SediGetInModel
    {
        public int Id { get; set; }
    }
    public class Az_SediGetOutModel : ModelResult
    {
        public Az_SediModel Az_Sedi { get; set; } = new Az_SediModel { Id = 0, IdAz_Anagrafica = 0 };
        public List<CheckObjOn_Id_Number> Az_SediAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
    }


    public class Az_SediPutInModel
    {
        public Az_SediModel Az_Sedi { get; set; } = new Az_SediModel { Id = 0, IdAz_Anagrafica = 0 };
        public List<CheckObjOn_Id_Number> Az_SediAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
    }
    public class Az_SediPutOutModel : ModelResult
    {
        public Az_SediModel Az_Sedi { get; set; } = new Az_SediModel { Id = 0, IdAz_Anagrafica = 0 };
        public List<CheckObjOn_Id_Number> Az_SediAttivita { get; set; } = new List<CheckObjOn_Id_Number>();
    }


    public class Az_SediDeleteInModel
    {
        public int Id { get; set; }
    }
    public class Az_SediDeleteOutModel : ModelResult
    {
        public Az_SediModel Az_Sedi { get; set; } = new Az_SediModel { Id = 0, IdAz_Anagrafica = 0 };
    }
}
