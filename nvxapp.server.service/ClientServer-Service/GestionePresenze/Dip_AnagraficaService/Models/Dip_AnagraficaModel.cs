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
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();
        public List<RoleCode> RoleCode { get; set; } = new List<RoleCode>();

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




}
