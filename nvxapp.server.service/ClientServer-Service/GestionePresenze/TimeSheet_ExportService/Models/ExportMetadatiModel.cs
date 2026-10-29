using System.Text.Json.Serialization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models
{
    /*
     Metadati dell'export verso le paghe (analisi ANALISI_EXPORT_AI.md, par. 4): i dati di tutti i dipendenti
     esportati, con le causali del modello raggruppate per codice di output e periodo (giorno o mese).
     Senza documentazione e' il file restituito (JSON); con documentazione e' l'ingresso del renderer della ricetta.
    */
    public class ExportMetadati
    {
        public int Versione { get; set; } = 1;
        public DateTime GeneratoIl { get; set; }
        public ExportMetadati_Azienda Azienda { get; set; } = new();
        public ExportMetadati_Modello ModelloExport { get; set; } = new();
        public ExportMetadati_Periodo Periodo { get; set; } = new();
        public List<ExportMetadati_CausaleEsportata> CausaliEsportate { get; set; } = new();
        public List<ExportMetadati_Dipendente> Dipendenti { get; set; } = new();
    }

    public class ExportMetadati_Azienda
    {
        public string Descrizione { get; set; } = string.Empty;
    }

    public class ExportMetadati_Modello
    {
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
    }

    public class ExportMetadati_Periodo
    {
        /// <summary>yyyy-MM-dd</summary>
        public string Dal { get; set; } = string.Empty;
        public string Al { get; set; } = string.Empty;
    }

    public class ExportMetadati_CausaleEsportata
    {
        public string Codice { get; set; } = string.Empty;
        public string TipoElaborazione { get; set; } = string.Empty;
        public string TipoUnita { get; set; } = string.Empty;
    }

    public class ExportMetadati_Dipendente
    {
        public ExportMetadati_Anagrafica Anagrafica { get; set; } = new();
        public List<ExportMetadati_Causale> Causali { get; set; } = new();
    }

    public class ExportMetadati_Anagrafica
    {
        public int IdDipendente { get; set; }
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        public List<ExportMetadati_Rapporto> Rapporti { get; set; } = new();
    }

    public class ExportMetadati_Rapporto
    {
        public string? DataAssunzione { get; set; }
        public string? DataLicenziamento { get; set; }
        public string TipoContratto { get; set; } = string.Empty;
        public decimal? OreSettimanaliContratto { get; set; }
        public string? Ccnl { get; set; }
    }

    public class ExportMetadati_Causale
    {
        /// <summary>codice di output e periodo, es. "ORD|2026-10" (mensile) o "FER|2026-10-03" (giornaliera)</summary>
        public string Chiave { get; set; } = string.Empty;
        public string Codice { get; set; } = string.Empty;
        /// <summary>yyyy-MM-dd (giornaliera) o yyyy-MM (mensile)</summary>
        public string Periodo { get; set; } = string.Empty;
        public string TipoElaborazione { get; set; } = string.Empty;
        public string TipoUnita { get; set; } = string.Empty;
        /// <summary>ore decimali, giorni o euro, 2 decimali; puo' essere negativo per il Verso Sottrazione</summary>
        public decimal Valore { get; set; }
    }

    /*
     Ricetta del formato prodotta dall'AI dalla documentazione (par. 6.3): il server la applica ai metadati reali
     (ExportRicettaRenderer). Righe di dettaglio: una per ogni causale di ogni dipendente (RigaPer "causale"), oppure una
     per dipendente con i Campi del dipendente seguiti dai CampiCausale ripetuti per ogni sua causale (RigaPer "dipendente");
     intestazione e piede facoltativi.
    */
    public class ExportRicetta
    {
        /// <summary>"testo" (TXT/CSV/tracciato fisso) oppure "xlsx"</summary>
        public string Formato { get; set; } = "testo";
        /// <summary>estensione del file (senza punto), tra quelle ammesse dai file dei job</summary>
        public string Estensione { get; set; } = "txt";
        /// <summary>"utf-8", "utf-8-bom", "windows-1252" (ANSI), "ascii"</summary>
        public string Codifica { get; set; } = "utf-8";
        /// <summary>"CRLF" oppure "LF"</summary>
        public string FineRiga { get; set; } = "CRLF";
        /// <summary>separatore dei campi (es. ";" o "\t"); null o vuoto = tracciato a larghezza fissa</summary>
        public string? Separatore { get; set; }
        /// <summary>prima riga con i titoli delle colonne (Titolo dei campi)</summary>
        public bool TitoliColonne { get; set; }
        /// <summary>riga di dettaglio: "causale" (una per causale di ogni dipendente) o "dipendente" (una per dipendente)</summary>
        public string RigaPer { get; set; } = "causale";
        /// <summary>campi della riga di dettaglio (con "dipendente": solo i dati del dipendente, all'inizio della riga)</summary>
        public List<ExportRicetta_Campo> Campi { get; set; } = new();
        /// <summary>solo con "dipendente": campi ripetuti per ogni causale del dipendente, uno dopo l'altro nella stessa riga</summary>
        public List<ExportRicetta_Campo> CampiCausale { get; set; } = new();
        /// <summary>righe prima del dettaglio (ognuna e' una lista di campi)</summary>
        public List<List<ExportRicetta_Campo>> Intestazione { get; set; } = new();
        /// <summary>righe dopo il dettaglio</summary>
        public List<List<ExportRicetta_Campo>> Piede { get; set; } = new();
        /// <summary>cose della documentazione che la ricetta non sa esprimere o dati mancanti</summary>
        public List<string> Avvisi { get; set; } = new();
    }

    public class ExportRicetta_Campo
    {
        /// <summary>sorgente del valore (vedi ExportRicettaRenderer.Sorgenti), es. "anagrafica.matricola", "causale.valore", "costante"</summary>
        public string Campo { get; set; } = string.Empty;
        /// <summary>valore fisso per "costante"</summary>
        public string? Valore { get; set; }
        /// <summary>titolo della colonna (TitoliColonne / xlsx)</summary>
        public string? Titolo { get; set; }
        /// <summary>lunghezza fissa del campo (tracciato a larghezza fissa); null = libera</summary>
        public int? Lunghezza { get; set; }
        /// <summary>"sinistra" (default per i testi) o "destra" (default per i numeri)</summary>
        public string? Allinea { get; set; }
        /// <summary>carattere di riempimento (default spazio)</summary>
        public string? Riempi { get; set; }
        /// <summary>formato delle date (es. "ddMMyyyy", "yyyy-MM-dd", "MMyyyy")</summary>
        public string? Formato { get; set; }
        /// <summary>decimali dei numeri (default 2)</summary>
        public int? Decimali { get; set; }
        /// <summary>separatore dei decimali: "," "." oppure "" (nessuno: 12,50 -> 1250)</summary>
        public string? SeparatoreDecimali { get; set; }
        /// <summary>segno dei negativi: "-" (default, davanti), "finale" (dopo il numero), "nessuno"</summary>
        public string? Segno { get; set; }

        [JsonIgnore]
        public bool IsNumero => Campo is "causale.valore" or "totale.valore" or "totale.righe" or "anagrafica.idDipendente";
    }
}
