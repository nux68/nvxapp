using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models
{
    public class Par_ProfiloOrarioModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public int NumGiorniCiclo { get; set; }
        public TipoProfilo TipoProfilo { get; set; }
        public int idPar_Orario_Festivo { get; set; }
        public bool EccedenzeAttive { get; set; }
        public StraoTipoConteggio TipoConteggio { get; set; }
        public TimeOnly? StraoSogliaHHFullTime { get; set; }
        public decimal SogliaSettimanaleFT { get; set; }
        public decimal SupplLimitePerc { get; set; }
        public TimeRoundInterval EccArrotondamento { get; set; }
        public int EccFranchigiaMin { get; set; }
        public bool AssenzeContanoSoglia { get; set; }
        public bool StraoSoloAutorizzato { get; set; }
        public bool DomenicaFestiva { get; set; }
        public TimeOnly NotturnoDalle { get; set; }
        public TimeOnly NotturnoAlle { get; set; }


        public int IdCausale_Lavoro_Strao { get; set; }
        public int? IdCausale_Lavoro_StraoNott { get; set; }
        public int? IdCausale_Lavoro_StraoFest { get; set; }
        public int? IdCausale_Lavoro_StraoFestNott { get; set; }
        public int IdCausale_Lavoro_Suppl { get; set; }
        public int? IdCausale_Lavoro_SupplEcc { get; set; }
        public int? IdCausale_Lavoro_EccOrd { get; set; }
        public int? IdCausale_BancaOre { get; set; }
        public int IdGiustificativo_Assenza_Ingiust { get; set; }

    }

    public class Par_ProfiloOrario_GetAllInModel { }
    public class Par_ProfiloOrario_GetAllOutModel : ModelResult
    {
        public List<Par_ProfiloOrarioModel> Par_ProfiloOrario { get; set; } = new List<Par_ProfiloOrarioModel>();
    }

    public class Par_ProfiloOrario_GetInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloOrario_GetOutModel : ModelResult
    {
        public Par_ProfiloOrarioModel? Par_ProfiloOrario { get; set; }
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }

    public class Par_ProfiloOrario_PutInModel
    {
        public Par_ProfiloOrarioModel Par_ProfiloOrario { get; set; } = new Par_ProfiloOrarioModel();
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }
    public class Par_ProfiloOrario_PutOutModel : ModelResult
    {
        public Par_ProfiloOrarioModel? Par_ProfiloOrario { get; set; }
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }

    public class Par_ProfiloOrario_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloOrario_DeleteOutModel : ModelResult { }
}
