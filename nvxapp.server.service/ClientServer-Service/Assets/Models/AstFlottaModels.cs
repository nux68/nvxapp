using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Assets.Models
{
    // ------------------------------------------------------------------------------------------------ comuni del modulo
    public class Ast_IdInModel { public int Id { get; set; } }

    public class Ast_LookupModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Attivo { get; set; } = true;
    }

    // dipendente (Common) da scegliere nelle assegnazioni
    public class Ast_DipendenteModel
    {
        public int Id { get; set; }
        public string Nominativo { get; set; } = string.Empty;
        public bool HaUtente { get; set; }
        public bool Attivo { get; set; }
        public DateTime? DataCessazione { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ flotta (AST-FLO)
    public class Ast_VeicoloModel
    {
        public int Id { get; set; }
        public string Targa { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modello { get; set; }
        public int Alimentazione { get; set; }
        public DateTime? DataImmatricolazione { get; set; }
        public int Possesso { get; set; }
        public string? Fornitore { get; set; }
        public string? NumeroContratto { get; set; }
        public DateTime? ScadenzaContratto { get; set; }
        public int? KmInclusi { get; set; }
        public decimal? CanoneMensile { get; set; }
        public int? IdAz_Sedi { get; set; }
        public int Uso { get; set; }
        public decimal CostoKmAci { get; set; }
        public int? EmissioniCO2 { get; set; }
        public int? KmAttuali { get; set; }
        public int Stato { get; set; }
        public string? Note { get; set; }
    }

    public class Ast_VeicoloListModel
    {
        public int Id { get; set; }
        public string Targa { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public int Uso { get; set; }
        public int Stato { get; set; }
        public string? Sede { get; set; }
        public string? Assegnatario { get; set; }
        public DateTime? ProssimaScadenza { get; set; }
        public string? ProssimaScadenzaDescrizione { get; set; }
    }

    public class Ast_VeicoloAssegnazioneModel
    {
        public int Id { get; set; }
        public int IdAst_Veicolo { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public bool UsoPromiscuo { get; set; } = true;
        /// <summary>0 = dalle emissioni del veicolo</summary>
        public decimal PercentualeFringe { get; set; }
        /// <summary>rimborso al km dell'autovettura del dipendente in Common (note spese delle Presenze)</summary>
        public decimal RimborsoKm { get; set; }
        public int? IdDip_RapportoAutovetture { get; set; }
        public string? Note { get; set; }
        /// <summary>calcolati: fringe benefit annuo e quota dell'anno in corso (giorni del periodo nell'anno)</summary>
        public decimal FringeAnnuo { get; set; }
        public decimal FringeAnnoCorrente { get; set; }
    }

    public class Ast_VeicoloScadenzaModel
    {
        public int Id { get; set; }
        public int IdAst_Veicolo { get; set; }
        public int Tipo { get; set; }
        public string? Descrizione { get; set; }
        public DateTime DataScadenza { get; set; }
        public int? Km { get; set; }
        public decimal? Importo { get; set; }
        public DateTime? DataEsecuzione { get; set; }
        public string? Note { get; set; }
    }

    public class Ast_VeicoloCostoModel
    {
        public int Id { get; set; }
        public int IdAst_Veicolo { get; set; }
        public DateTime Data { get; set; }
        public int Tipo { get; set; }
        public decimal Importo { get; set; }
        public int? Km { get; set; }
        public string? Descrizione { get; set; }
        public string? Documento { get; set; }
    }

    public class Ast_MultaModel
    {
        public int Id { get; set; }
        public int IdAst_Veicolo { get; set; }
        public string? Targa { get; set; }
        public DateTime DataInfrazione { get; set; }
        public string? NumeroVerbale { get; set; }
        public string? Luogo { get; set; }
        public string? Descrizione { get; set; }
        public decimal Importo { get; set; }
        /// <summary>null in inserimento = conducente dal periodo di assegnazione alla data dell'infrazione</summary>
        public int? IdDip_Anagrafica { get; set; }
        public string? Conducente { get; set; }
        public int Stato { get; set; }
        public DateTime? DataNotifica { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string? Note { get; set; }
    }

    public class Ast_VeicoloListInModel { public bool Dismessi { get; set; } }
    public class Ast_VeicoloListOutModel : ModelResult
    {
        public Ast_PermessiModel Permessi { get; set; } = new();
        public List<Ast_VeicoloListModel> Veicolo { get; set; } = new();
    }

    public class Ast_VeicoloGetOutModel : ModelResult
    {
        public Ast_VeicoloModel Veicolo { get; set; } = new();
        public List<Ast_VeicoloAssegnazioneModel> Assegnazione { get; set; } = new();
        public List<Ast_VeicoloScadenzaModel> Scadenza { get; set; } = new();
        public List<Ast_VeicoloCostoModel> Costo { get; set; } = new();
        public List<Ast_MultaModel> Multa { get; set; } = new();
        public decimal TotaleCostiAnno { get; set; }
        public List<Ast_LookupModel> Sede { get; set; } = new();
        public List<Ast_DipendenteModel> Dipendente { get; set; } = new();
    }

    public class Ast_VeicoloPutInModel { public Ast_VeicoloModel Veicolo { get; set; } = new(); }
    public class Ast_VeicoloPutOutModel : ModelResult { public Ast_VeicoloModel Veicolo { get; set; } = new(); }

    public class Ast_AssegnazionePutInModel { public Ast_VeicoloAssegnazioneModel Assegnazione { get; set; } = new(); }
    public class Ast_AssegnazionePutOutModel : ModelResult { public Ast_VeicoloAssegnazioneModel Assegnazione { get; set; } = new(); }

    public class Ast_VeicoloScadenzaPutInModel { public Ast_VeicoloScadenzaModel Scadenza { get; set; } = new(); }
    public class Ast_VeicoloScadenzaPutOutModel : ModelResult { public Ast_VeicoloScadenzaModel Scadenza { get; set; } = new(); }

    public class Ast_VeicoloCostoPutInModel { public Ast_VeicoloCostoModel Costo { get; set; } = new(); }
    public class Ast_VeicoloCostoPutOutModel : ModelResult { public Ast_VeicoloCostoModel Costo { get; set; } = new(); }

    public class Ast_MultaPutInModel { public Ast_MultaModel Multa { get; set; } = new(); }
    public class Ast_MultaPutOutModel : ModelResult { public Ast_MultaModel Multa { get; set; } = new(); }

    public class Ast_DeleteOutModel : ModelResult { public bool Eliminato { get; set; } }

    // veicolo e multe dell'utente collegato
    public class Ast_MieiVeicoliOutModel : ModelResult
    {
        public List<Ast_MioVeicoloModel> Veicolo { get; set; } = new();
        public List<Ast_MultaModel> Multa { get; set; } = new();
    }

    public class Ast_MioVeicoloModel
    {
        public int Id { get; set; }
        public string Targa { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public bool UsoPromiscuo { get; set; }
        public decimal FringeAnnuo { get; set; }
        public decimal FringeAnnoCorrente { get; set; }
    }
}
