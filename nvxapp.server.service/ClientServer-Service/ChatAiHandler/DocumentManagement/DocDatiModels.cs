namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.DocumentManagement
{
    // Dati strutturati dei comandi di lettura del Documentale (CommandResult.Data), per l'AI esterna (MCP-33,
    // ANALISI_INFRASTRUCTURE_MCP.md): date ISO yyyy-MM-dd, mai il contenuto dei file.

    // DocList: documenti trovati
    public class DocElencoDati
    {
        public string? Filtro { get; set; }
        public int Totale { get; set; }
        // elenco limitato ai documenti più recenti: restringere i filtri
        public bool Troncato { get; set; }
        public List<DocDocumentoDati> Documenti { get; set; } = new();
    }

    public class DocDocumentoDati
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        // Personale, Azienda, Cliente
        public string Categoria { get; set; } = string.Empty;
        public string? Cartella { get; set; }
        // dipendente o cliente a cui si riferisce il documento
        public string? Dipendente { get; set; }
        public string? Cliente { get; set; }
        public string? DataDocumento { get; set; }
        // mese di competenza yyyy-MM (es. cedolino)
        public string? Competenza { get; set; }
        public string? DataScadenza { get; set; }
        public bool Scaduto { get; set; }
        // Bozza, Pubblicato, Archiviato
        public string Stato { get; set; } = string.Empty;
        public int Versione { get; set; }
        public string? DataCaricamento { get; set; }
        // per l'utente collegato come destinatario: mai aperto, presa visione e firma richieste e fatte
        public bool NuovoPerMe { get; set; }
        public bool PresaVisioneRichiestaAMe { get; set; }
        public string? MiaDataPresaVisione { get; set; }
        public bool FirmaRichiestaAMe { get; set; }
        public string? MiaDataFirma { get; set; }
        // firma richiesta ai destinatari del documento: quanti devono firmare e quanti l'hanno fatto
        public bool FirmaRichiesta { get; set; }
        public int Firmatari { get; set; }
        public int Firmati { get; set; }
        public bool OltreConservazione { get; set; }
    }

    // DocSignatures: firme richieste nel periodo
    public class DocFirmeDati
    {
        public string Dal { get; set; } = string.Empty;
        public string Al { get; set; } = string.Empty;
        public string? Filtro { get; set; }
        public int DaFirmare { get; set; }
        public int Firmate { get; set; }
        // elenco limitato alle richieste più recenti: restringere il periodo
        public bool Troncato { get; set; }
        public List<DocFirmaDati> Firme { get; set; } = new();
    }

    public class DocFirmaDati
    {
        public int IdDocumento { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Dipendente { get; set; }
        public string? Cliente { get; set; }
        public string? DataDocumento { get; set; }
        public string? Competenza { get; set; }
        public string? DataScadenza { get; set; }
        // destinatario a cui è chiesta la firma; Mia = è l'utente collegato
        public string Destinatario { get; set; } = string.Empty;
        public bool Mia { get; set; }
        // "daFirmare" o "firmato"
        public string Stato { get; set; } = string.Empty;
        public string? DataRichiesta { get; set; }
        public string? DataFirma { get; set; }
        public int? VersioneFirmata { get; set; }
        public int VersioneCorrente { get; set; }
    }
}
