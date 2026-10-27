using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models
{
    // spesa (dettaglio) di una nota spese, con i dati della richiesta che la contiene
    public class Dip_GG_NotaSpesaModel : HashModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public int? IdDip_GG_Richiesta { get; set; }
        public string RichiestaDescrizione { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int IdPar_TipoSpesa { get; set; }
        public decimal Importo { get; set; }
        public string? Descrizione { get; set; }
        // rimborso km: km percorsi e autovettura usata
        public decimal? Km { get; set; }
        public int? IdDip_RapportoAutovetture { get; set; }
        public string? AutovetturaDescrizione { get; set; }
        public string? AutovetturaTarga { get; set; }
        public StatoRichiesta RichiestaStato { get; set; }
        public List<Dip_GG_NotaSpesa_AllegatoModel> Allegati { get; set; } = new List<Dip_GG_NotaSpesa_AllegatoModel>();
    }

    // allegato senza contenuto (si scarica a parte)
    public class Dip_GG_NotaSpesa_AllegatoModel
    {
        public int Id { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int Dimensione { get; set; }
    }

    public class Dip_GG_NotaSpesaInModel
    {
    }

    public class Dip_GG_NotaSpesaOutModel : ModelResult
    {
    }

    // spese di una richiesta (dipendente proprietario o amministratori)
    public class Dip_GG_NotaSpesa_Get4Richiesta_InModel
    {
        public int IdDip_GG_Richiesta { get; set; }
    }
    public class Dip_GG_NotaSpesa_Get4Richiesta_OutModel : ModelResult
    {
        /// <summary>la richiesta (nota spese) con stato e revoca, per decidere approvazione e rifiuto</summary>
        public Dip_GG_RichiestaModel? Dip_GG_Richiesta { get; set; }
        public List<Dip_GG_NotaSpesaModel> Dip_GG_NotaSpesa { get; set; } = new List<Dip_GG_NotaSpesaModel>();
    }

    // ricerca delle spese (CompanyPowerAdmin): periodo, dipendenti, tipo di spesa, stati
    public class Dip_GG_NotaSpesa_Search_InModel
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public List<string> UsersId { get; set; } = new List<string>();
        public int? IdPar_TipoSpesa { get; set; }
        public List<StatoRichiesta>? Stati { get; set; }
    }
    public class Dip_GG_NotaSpesa_Search_OutModel : ModelResult
    {
        public List<Dip_GG_NotaSpesaModel> Dip_GG_NotaSpesa { get; set; } = new List<Dip_GG_NotaSpesaModel>();
    }

    // contenuto di un allegato
    public class Dip_GG_NotaSpesa_Allegato_Get_InModel
    {
        public int Id { get; set; }
    }
    public class Dip_GG_NotaSpesa_Allegato_Get_OutModel : ModelResult
    {
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        /// <summary>contenuto del file in base64</summary>
        public string Contenuto { get; set; } = string.Empty;
    }
}
