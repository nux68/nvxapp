using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models
{
    public class Par_OrarioModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public int NumeroCoppie { get; set; }
        public int? IdCausale_HH_Lav_MonteOre { get; set; }
        public OrarioTimbratureTipo TimbratureTipo { get; set; } = 0;
        public TimeOnly Hh_Teo_MonteOre { get; set; } = new TimeOnly(0, 0);
        //public int? IdAz_SubCommessaAttivita_MonteOre { get; set; }
    }

    public class Par_OrarioPausaModel
    {
        public int Id { get; set; }
        public int IdPar_Orario { get; set; }
        public int NumPausa { get; set; } = 1;
        public PausaTipo Tipo { get; set; } = PausaTipo.Pranzo;
        public string? Descrizione { get; set; }
        public PausaModalita Modalita { get; set; } = PausaModalita.Rilevata;
        public int DurataMin { get; set; }
        public int? DurataMaxMin { get; set; }
        public int SogliaMin { get; set; } = 360;
        public TimeOnly? FinestraDalle { get; set; }
        public TimeOnly? FinestraAlle { get; set; }
        public int RetribuitaMin { get; set; }
        public int? IdCausale_Pausa { get; set; }
    }

    public class Par_Orario_GetAllInModel { }
    public class Par_Orario_GetAllOutModel : ModelResult
    {
        public List<Par_OrarioModel> Par_Orario { get; set; } = new List<Par_OrarioModel>();
    }




    public class Par_Orario_GetInModel
    {
        public int Id { get; set; }
    }
    public class Par_Orario_GetOutModel : ModelResult
    {
        public Par_OrarioModel? Par_Orario { get; set; }

        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
        public List<Par_OrarioPausaModel> Par_OrarioPausa { get; set; } = new List<Par_OrarioPausaModel>();

    }

    public class Par_Orario_PutInModel
    {
        public Par_OrarioModel Par_Orario { get; set; } = new Par_OrarioModel();
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
        public List<Par_OrarioPausaModel> Par_OrarioPausa { get; set; } = new List<Par_OrarioPausaModel>();
    }
    public class Par_Orario_PutOutModel : ModelResult
    {
        public Par_OrarioModel? Par_Orario { get; set; }
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
        public List<Par_OrarioPausaModel> Par_OrarioPausa { get; set; } = new List<Par_OrarioPausaModel>();
    }

    public class Par_Orario_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Orario_DeleteOutModel : ModelResult { }
}
