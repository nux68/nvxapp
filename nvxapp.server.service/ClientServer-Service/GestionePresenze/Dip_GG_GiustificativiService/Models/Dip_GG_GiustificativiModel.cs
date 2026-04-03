using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models
{
    public class Dip_GG_GiustificativiModel : HashModel
    {
        [HashField]
        public int Id { get; set; }
        [HashField]
        public int IdDip_RapportoLavoro { get; set; }
        [HashField]
        public DateTime Data { get; set; }
        [HashField]
        public int IdJustificationType { get; set; }
        [HashField]
        public JustificationInputType InputType { get; set; }
        [HashField]
        public TimeSpan? Hours { get; set; }  // se InputType=manual
        [HashField]
        public TimeSpan? From { get; set; }   // se InputType=manual  (dalle)
        [HashField]
        public required int IdPar_Giustificativi { get; set; }
        [HashField]
        public StatoRichiesta RichiestaStato { get; set; }
        [HashField]
        public int? IdDip_GG_Richiesta { get; set; }


    }

    public class Dip_GG_Giustificativi_GetAll_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }

    public class Dip_GG_Giustificativi_GetAll_OutModel : ModelResult
    {
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();


        public Dip_GG_Giustificativi_GetAll_OutModel()
        {

        }
    }




    public class Dip_GG_Giustificativi_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_GG_Giustificativi_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
    }


    public class Dip_GG_GiustificativiGetInModel 
    {
        public int Id { get; set; }

        /* per inizializzare il record nuovo */
        public required int IdDip_RapportoLavoro { get; set; }


        public DateTime? Data { get; set; }
    }
    public class Dip_GG_GiustificativiGetOutModel : ModelResult
    {
        public Dip_GG_GiustificativiModel Dip_GG_Giustificativi { get; set; } = new Dip_GG_GiustificativiModel(){ IdPar_Giustificativi = 0 };
    }


    public class Dip_GG_GiustificativiPutInModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public Dip_GG_GiustificativiModel Dip_GG_Giustificativi { get; set; } = new Dip_GG_GiustificativiModel() { Id = 0, IdDip_RapportoLavoro = 0, IdPar_Giustificativi = 0 };
    }
    public class Dip_GG_GiustificativiPutOutModel : ModelResult
    {
        public Dip_GG_GiustificativiModel Dip_GG_Giustificativi { get; set; } = new Dip_GG_GiustificativiModel() { Id = 0, IdDip_RapportoLavoro = 0, IdPar_Giustificativi = 0 };
    }

    
    public class Dip_GG_Giustificativi_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Dip_GG_Giustificativi_DeleteOutModel : ModelResult { }


}
