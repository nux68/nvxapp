using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models
{
    public class Dip_AnagraficaModel
    {
        public string UserName { get; set; } = string.Empty;
        public string IdAspNetUsers { get; set; } = string.Empty;
        public int Id { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public List<RoleCode> RoleCode { get; set; } = new List<RoleCode>();

        // questi vengono usati per visualizzare i dati, ma le editazioni lavorano dievrsamente con propri ogegtti
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();

    }

    public class Dip_Anagrafica_GetAll_InModel
    {

    }

    public class Dip_Anagrafica_GetAll_OutModel : ModelResult
    {
        public List<Dip_AnagraficaModel> Dip_Anagrafica { get; set; } = new List<Dip_AnagraficaModel>();

        public Dip_Anagrafica_GetAll_OutModel()
        {

        }
    }




    public class Dip_Anagrafica4EditModel:Dip_AnagraficaModel
    {
        
        public string Descrizione { get; set; } = string.Empty;
        public int IdUserCompany { get; set; }
        public string? RoleId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

    }
    
    public class Dip_Anagrafica_Get_InModel
    {
        public string Id { get; set; }  = string.Empty; // = IdAspNetUsers
    }
    public class Dip_Anagrafica_Get_OutModel : ModelResult
    {
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }

    public class Dip_Anagrafica_Put_InModel
    {
        public string Id { get; set; } = string.Empty;// = IdAspNetUsers
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }
    public class Dip_Anagrafica_Put_OutModel : ModelResult
    {
        public string Id { get; set; } = string.Empty;// = IdAspNetUsers
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }


}
