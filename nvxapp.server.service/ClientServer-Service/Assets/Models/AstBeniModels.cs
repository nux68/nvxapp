using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Assets.Models
{
    // ------------------------------------------------------------------------------------------------ beni assegnati (AST-BENI)
    public class Ast_CategoriaModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public bool Attivo { get; set; } = true;
        public int Ordine { get; set; }
        public int NumeroBeni { get; set; }
    }

    public class Ast_BeneModel
    {
        public int Id { get; set; }
        public string CodiceInventario { get; set; } = string.Empty;
        public int IdAst_Categoria { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? MarcaModello { get; set; }
        public string? NumeroSerie { get; set; }
        public DateTime? DataAcquisto { get; set; }
        public decimal? Valore { get; set; }
        public DateTime? ScadenzaGaranzia { get; set; }
        public int? IdAz_Sedi { get; set; }
        public int Stato { get; set; }
        public string? Note { get; set; }
    }

    public class Ast_BeneListModel
    {
        public int Id { get; set; }
        public string CodiceInventario { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public string? NumeroSerie { get; set; }
        public string? Sede { get; set; }
        public int Stato { get; set; }
        public string? Assegnatario { get; set; }
        public DateTime? DataConsegna { get; set; }
        public DateTime? ScadenzaGaranzia { get; set; }
    }

    // stato del verbale (presa visione) di consegna o restituzione
    public class Ast_VerbaleModel
    {
        /// <summary>false = non richiesto (dipendente senza utente)</summary>
        public bool Richiesto { get; set; }
        public DateTime? DataConferma { get; set; }
        public string? Canale { get; set; }
    }

    public class Ast_BeneAssegnazioneModel
    {
        public int Id { get; set; }
        public int IdAst_Bene { get; set; }
        public string? Bene { get; set; }
        public string? CodiceInventario { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public DateTime DataConsegna { get; set; }
        public string? CondizioniConsegna { get; set; }
        public DateTime? DataRestituzione { get; set; }
        public string? CondizioniRestituzione { get; set; }
        public string? Note { get; set; }
        public Ast_VerbaleModel VerbaleConsegna { get; set; } = new();
        public Ast_VerbaleModel VerbaleRestituzione { get; set; } = new();
    }

    public class Ast_CategoriaListOutModel : ModelResult { public List<Ast_CategoriaModel> Categoria { get; set; } = new(); }
    public class Ast_CategoriaPutInModel { public Ast_CategoriaModel Categoria { get; set; } = new(); }
    public class Ast_CategoriaPutOutModel : ModelResult { public Ast_CategoriaModel Categoria { get; set; } = new(); }

    public class Ast_BeneListInModel { public bool Dismessi { get; set; } }
    public class Ast_BeneListOutModel : ModelResult
    {
        public Ast_PermessiModel Permessi { get; set; } = new();
        public List<Ast_BeneListModel> Bene { get; set; } = new();
    }

    public class Ast_BeneGetOutModel : ModelResult
    {
        public Ast_BeneModel Bene { get; set; } = new();
        public List<Ast_BeneAssegnazioneModel> Assegnazione { get; set; } = new();
        public List<Ast_CategoriaModel> Categoria { get; set; } = new();
        public List<Ast_LookupModel> Sede { get; set; } = new();
        public List<Ast_DipendenteModel> Dipendente { get; set; } = new();
    }

    public class Ast_BenePutInModel { public Ast_BeneModel Bene { get; set; } = new(); }
    public class Ast_BenePutOutModel : ModelResult { public Ast_BeneModel Bene { get; set; } = new(); }

    // consegna di un bene disponibile a un dipendente (con il verbale)
    public class Ast_ConsegnaPutInModel
    {
        public int IdAst_Bene { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public DateTime DataConsegna { get; set; }
        public string? CondizioniConsegna { get; set; }
        public string? Note { get; set; }
    }

    // restituzione: chiude l'assegnazione e porta il bene nello stato indicato (disponibile, in riparazione, dismesso)
    public class Ast_RestituzionePutInModel
    {
        public int IdAst_BeneAssegnazione { get; set; }
        public DateTime DataRestituzione { get; set; }
        public string? CondizioniRestituzione { get; set; }
        public int StatoBene { get; set; }
    }

    public class Ast_BeneAssegnazionePutOutModel : ModelResult { public Ast_BeneAssegnazioneModel Assegnazione { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ restituzioni alla cessazione (AST-BENI.3)
    public class Ast_DaRestituireModel
    {
        /// <summary>"bene" o "veicolo"</summary>
        public string Tipo { get; set; } = string.Empty;
        public int IdAssegnazione { get; set; }
        public int IdOggetto { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public int IdDip_Anagrafica { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public DateTime DataCessazione { get; set; }
        public DateTime DataConsegna { get; set; }
        public DateTime? DataAvviso { get; set; }
        /// <summary>chiave unica nell'elenco (tipo-id)</summary>
        public string Chiave => Tipo + "-" + IdAssegnazione;
    }

    public class Ast_DaRestituireListOutModel : ModelResult { public List<Ast_DaRestituireModel> DaRestituire { get; set; } = new(); }

    public class Ast_DaRestituireVerificaOutModel : ModelResult
    {
        /// <summary>assegnazioni avvisate ora (nuove)</summary>
        public int Avvisate { get; set; }
        public List<Ast_DaRestituireModel> DaRestituire { get; set; } = new();
    }

    // ------------------------------------------------------------------------------------------------ utente
    public class Ast_MieiBeniOutModel : ModelResult { public List<Ast_BeneAssegnazioneModel> Assegnazione { get; set; } = new(); }
}
