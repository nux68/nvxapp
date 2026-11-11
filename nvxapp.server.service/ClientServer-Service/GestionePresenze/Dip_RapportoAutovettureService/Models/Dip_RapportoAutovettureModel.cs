using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService.Models
{
    // autovettura del dipendente per le spese di rimborso chilometrico (tabella comune, AT-COM: prima era del rapporto di lavoro)
    public class Dip_RapportoAutovettureModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public string? Targa { get; set; }
        public decimal RimborsoKM { get; set; }
    }

    public class Dip_RapportoAutovetture_Get_InModel
    {
        public int Id { get; set; }  // = IdDip_Anagrafica
    }
    public class Dip_RapportoAutovetture_Get_OutModel : ModelResult
    {
        public List<Dip_RapportoAutovettureModel> Dip_RapportoAutovetture { get; set; } = new List<Dip_RapportoAutovettureModel>();
    }

    public class Dip_RapportoAutovetture_Put_InModel
    {
        public int Id { get; set; } // = IdDip_Anagrafica
        public List<Dip_RapportoAutovettureModel> Dip_RapportoAutovetture { get; set; } = new List<Dip_RapportoAutovettureModel>();
    }
    public class Dip_RapportoAutovetture_Put_OutModel : ModelResult
    {
        public int Id { get; set; } // = IdDip_Anagrafica
        public List<Dip_RapportoAutovettureModel> Dip_RapportoAutovetture { get; set; } = new List<Dip_RapportoAutovettureModel>();
    }

    // autovetture del dipendente corrente (nota spese)
    public class Dip_RapportoAutovetture_GetAll4User_InModel
    {
    }
    public class Dip_RapportoAutovetture_GetAll4User_OutModel : ModelResult
    {
        public List<Dip_RapportoAutovettureModel> Dip_RapportoAutovetture { get; set; } = new List<Dip_RapportoAutovettureModel>();
    }
}
