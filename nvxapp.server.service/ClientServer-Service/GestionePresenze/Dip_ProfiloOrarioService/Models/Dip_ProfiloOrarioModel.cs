using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models
{
    public class Dip_ProfiloOrarioModel
    {
         public int Id { get; set; }
         public int IdDip_RapportoLavoro { get; set; }
         public int IdPar_ProfiloOrario { get; set; }
         public int NumGiornoPartenzaCiclo { get; set; }
         public DateTime Dal { get; set; }
         public DateTime Al { get; set; }
            
    }

    
    public class Dip_ProfiloOrario_Get_InModel
    {
        public int Id { get; set; }  // = IdDip_RapportoLavoro
    }
    public class Dip_ProfiloOrario_Get_OutModel : ModelResult
    {
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }

    public class Dip_ProfiloOrario_Put_InModel
    {
        public int Id { get; set; } // = IdDip_RapportoLavoro
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }
    public class Dip_ProfiloOrario_Put_OutModel : ModelResult
    {
        public int Id { get; set; } // = IdDip_RapportoLavoro
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }


    public class Dip_ProfiloOrario_Get_Profile_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    // Una riga orario del giorno, corrispondente a una riga Par_ProfiloOrarioGG.
    // ZOrder 1 = orario base (sempre presente), ZOrder > 1 = override condizionale.
    public class Dip_ProfiloOrario_DaySlot_GG
    {
        public int ZOrder      { get; set; } // ordinamento: 1 = default, 2+ = override
        public int IdPar_Orario { get; set; }
    }

    // Un record per ogni combinazione dipendente × giorno con tutte le righe orario ordinate per ZOrder
    public class Dip_ProfiloOrario_DaySlot
    {
        public string   IdAspNetUsers        { get; set; } = string.Empty;
        public int      IdDip_RapportoLavoro { get; set; }
        public DateTime Data                 { get; set; }
        public int      IdPar_ProfiloOrario  { get; set; }
        // righe orario del giorno ordinate per ZOrder (1=base, 2+=override)
        public List<Dip_ProfiloOrario_DaySlot_GG> Orari { get; set; } = new List<Dip_ProfiloOrario_DaySlot_GG>();
    }

    public class Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel : ModelResult
    {
        public List<Dip_ProfiloOrario_DaySlot> DaySlots { get; set; } = new List<Dip_ProfiloOrario_DaySlot>();
        public List<Par_OrarioModel> ParOrario            = new List<Par_OrarioModel>();
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH = new List<Par_OrarioIntervalloHHModel>();
    }

}
