using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models
{
    // ?? Calcolo contatori ?????????????????????????????????????????????????????

    public class Contatori_Calcolo_InModel
    {
        /// <summary>Rapporto di lavoro del dipendente.</summary>
        public int IdDip_RapportoLavoro { get; set; }

        /// <summary>Anno di riferimento.</summary>
        public int Anno { get; set; }

        /// <summary>Mese selezionato (1-12). Definisce i tre periodi.</summary>
        public int Mese { get; set; }
    }

    public class Contatori_Periodo
    {
        public string Maturato { get; set; } = "00:00:00";
        public string Goduto   { get; set; } = "00:00:00";
        public string Saldo    { get; set; } = "00:00:00";
    }

    public class Contatori_Giustificativo_Result
    {
        public int     IdPar_Giustificativi { get; set; }
        public string  Descrizione          { get; set; } = string.Empty;
        public string  Codice               { get; set; } = string.Empty;

        /// <summary>Mesi 1 .. Mese-1 + riporto (mese 0).</summary>
        public Contatori_Periodo PeriodoPrecedente  { get; set; } = new();

        /// <summary>Mese selezionato.</summary>
        public Contatori_Periodo PeriodoCorrente    { get; set; } = new();

        /// <summary>Mesi Mese+1 .. 12.</summary>
        public Contatori_Periodo PeriodoSuccessivo  { get; set; } = new();
    }

    public class Contatori_Calcolo_OutModel : ModelResult
    {
        public List<Contatori_Giustificativo_Result> Risultati { get; set; } = new();
    }

    // ?? Gestione riporto (mese 0) ?????????????????????????????????????????????

    public class Contatori_Riporto_Model
    {
        public int    Id                    { get; set; }
        public int    IdDip_RapportoLavoro  { get; set; }
        public int    IdPar_Giustificativi  { get; set; }
        public int    Anno                  { get; set; }
        /// <summary>Saldo riporto come stringa "HHH:mm:ss" compatibile con TimeSpan.Parse.</summary>
        public string SaldoRiporto          { get; set; } = "00:00:00";
        public bool   IsManuale             { get; set; }
    }

    public class Contatori_Riporto_GetAll_InModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public int Anno                 { get; set; }
    }

    public class Contatori_Riporto_GetAll_OutModel : ModelResult
    {
        public List<Contatori_Riporto_Model> Riporti { get; set; } = new();
    }

    public class Contatori_Riporto_Upsert_InModel
    {
        public Contatori_Riporto_Model Riporto { get; set; } = new();
    }

    public class Contatori_Riporto_Upsert_OutModel : ModelResult
    {
        public Contatori_Riporto_Model? Riporto { get; set; }
    }

    public class Contatori_Riporto_Delete_InModel
    {
        public int Id { get; set; }
    }

    public class Contatori_Riporto_Delete_OutModel : ModelResult { }

    // ?? Vista annuale (tutti i mesi) ??????????????????????????????????????????

    public class Contatori_Anno_InModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public int Anno                 { get; set; }
    }

    public class Contatori_Anno_MeseResult
    {
        public int Mese { get; set; }
        public List<Contatori_Giustificativo_Result> Risultati { get; set; } = new();
    }

    public class Contatori_Anno_OutModel : ModelResult
    {
        public List<Contatori_Anno_MeseResult> Mesi { get; set; } = new();
    }
}
