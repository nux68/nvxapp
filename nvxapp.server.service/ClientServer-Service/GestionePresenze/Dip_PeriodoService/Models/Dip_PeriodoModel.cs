using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models
{
    public class Dip_PeriodoModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public PeriodoStato Stato { get; set; }

        public int Min_Teo { get; set; }
        public int Min_Lav { get; set; }
        public int Min_Ord { get; set; }
        public int Min_Assenze { get; set; }
        public int Min_Ecc { get; set; }
        public int Min_Compensate { get; set; }
        public int Min_Provvisorie { get; set; }
        public int Min_Suppl { get; set; }
        public int Min_SupplEcc { get; set; }
        public int Min_Strao { get; set; }
        public int Min_StraoNott { get; set; }
        public int Min_StraoFest { get; set; }
        public int Min_StraoFestNott { get; set; }
        public int Min_EccOrd { get; set; }
        public int Min_NonRiconosciute { get; set; }
        public int Min_BancaOre { get; set; }
        public int Min_NonAutorizzate { get; set; }
        public int Min_DaAutorizzare { get; set; }
        public int? Min_Riconosciute { get; set; }

        public string? Note { get; set; }
        public DateTime? DataApprovazione { get; set; }
        public string? IdAspNetUsers_Approvazione { get; set; }
    }

    // dettaglio di un giorno con ore in piu' (drill-down "causa-effetto")
    public class Dip_Periodo_Giorno
    {
        public DateTime Data { get; set; }
        public bool Festivo { get; set; }
        public bool Bloccato { get; set; }
        public bool EccProvvisoria { get; set; }
        public bool BancaOre { get; set; }
        public int Min_Teo { get; set; }
        public int Min_Lav { get; set; }
        public int Min_Ecc { get; set; }
        public int Min_Suppl { get; set; }
        public int Min_SupplEcc { get; set; }
        public int Min_Strao { get; set; }
        public int Min_StraoNott { get; set; }
        public int Min_StraoFest { get; set; }
        public int Min_StraoFestNott { get; set; }
        public int Min_EccOrd { get; set; }
        public int Min_NonRiconosciute { get; set; }
        public int Min_NonAutorizzate { get; set; }
        public int Min_DaAutorizzare { get; set; }
    }

    public class Dip_Periodo_Settimana
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public bool Provvisoria { get; set; }
        public int Min_Teo { get; set; }
        public int Min_Lav { get; set; }
        public int Min_Ecc { get; set; }
        public int Min_Qualificate { get; set; }
    }

    public class Dip_Periodo_Get_InModel
    {
        public string? IdAspNetUsers { get; set; }   // vuoto = utente corrente
        public int Anno { get; set; }
        public int Mese { get; set; }
    }
    public class Dip_Periodo_Get_OutModel : ModelResult
    {
        public Dip_PeriodoModel? Dip_Periodo { get; set; }
        public TipoContratto TipoContratto { get; set; }
        public decimal? OreSettimanaliContratto { get; set; }
        public DestinazioneEccedenze DestinazioneEccedenze { get; set; }
        public StraoTipoConteggio TipoConteggio { get; set; }
        public bool EccedenzeAttive { get; set; }
        public int Min_Saldo { get; set; }           // lavorato - lavorabile fino a ieri
        public List<Dip_Periodo_Settimana> Settimane { get; set; } = new List<Dip_Periodo_Settimana>();
        public List<Dip_Periodo_Giorno> Giorni { get; set; } = new List<Dip_Periodo_Giorno>();
    }

    public class Dip_Periodo_GetAll_InModel
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public List<string> UsersId { get; set; } = new List<string>();
    }
    public class Dip_Periodo_Riga
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? UserName { get; set; }
        public TipoContratto TipoContratto { get; set; }
        public DestinazioneEccedenze DestinazioneEccedenze { get; set; }
        public Dip_PeriodoModel? Dip_Periodo { get; set; }
    }
    public class Dip_Periodo_GetAll_OutModel : ModelResult
    {
        public List<Dip_Periodo_Riga> Righe { get; set; } = new List<Dip_Periodo_Riga>();
    }

    public class Dip_Periodo_SetStato_InModel
    {
        public List<int> Id { get; set; } = new List<int>();
        public PeriodoStato Stato { get; set; }        // Approvato = approva, DaApprovare = riapri
    }
    public class Dip_Periodo_SetStato_OutModel : ModelResult
    {
        public List<Dip_PeriodoModel> Dip_Periodo { get; set; } = new List<Dip_PeriodoModel>();
    }

    public class Dip_Periodo_SetRiconosciute_InModel
    {
        public int Id { get; set; }
        public int? Min_Riconosciute { get; set; }   // null = tutte, 0 = nessuna
        public string? Note { get; set; }
    }
    public class Dip_Periodo_SetRiconosciute_OutModel : ModelResult
    {
        public Dip_PeriodoModel Dip_Periodo { get; set; } = new Dip_PeriodoModel();
    }
}
