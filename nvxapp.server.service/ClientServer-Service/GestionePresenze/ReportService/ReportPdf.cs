namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService
{
    /// <summary>
    /// Report pronto da scrivere (ReportPdfWriter): titolo, intestazione (etichetta/valore), una o piu' tabelle
    /// e note finali. Non dipende dalla pagina che lo chiede: ogni report di ReportService riempie questo modello.
    /// </summary>
    public class ReportPdf
    {
        public string Titolo { get; set; } = string.Empty;
        public List<KeyValuePair<string, string>> Intestazione { get; set; } = new List<KeyValuePair<string, string>>();
        // pagina orizzontale (tabelle larghe, es. piano ferie)
        public bool Orizzontale { get; set; }
        public List<ReportPdf_Tabella> Tabelle { get; set; } = new List<ReportPdf_Tabella>();
        // paragrafi in fondo (legende, avvisi)
        public List<string> Note { get; set; } = new List<string>();
    }

    public class ReportPdf_Tabella
    {
        public string? Titolo { get; set; }
        public List<ReportPdf_Colonna> Colonne { get; set; } = new List<ReportPdf_Colonna>();
        public List<ReportPdf_Riga> Righe { get; set; } = new List<ReportPdf_Riga>();
        // carattere piu' piccolo per le tabelle con molte colonne (punti; null = normale)
        public double? FontSize { get; set; }
    }

    public enum ReportPdf_Allineamento { Sinistra, Centro, Destra }

    public class ReportPdf_Colonna
    {
        public string Titolo { get; set; } = string.Empty;
        public double LarghezzaCm { get; set; }
        public ReportPdf_Allineamento Allineamento { get; set; } = ReportPdf_Allineamento.Sinistra;

        public ReportPdf_Colonna() { }
        public ReportPdf_Colonna(string titolo, double larghezzaCm, ReportPdf_Allineamento allineamento = ReportPdf_Allineamento.Sinistra)
        {
            Titolo = titolo;
            LarghezzaCm = larghezzaCm;
            Allineamento = allineamento;
        }
    }

    // Gruppo: riga di intestazione di un gruppo (es. dipendente); Totale: riga dei totali
    public enum ReportPdf_TipoRiga { Normale, Gruppo, Totale }

    public class ReportPdf_Riga
    {
        public ReportPdf_TipoRiga Tipo { get; set; } = ReportPdf_TipoRiga.Normale;
        public List<ReportPdf_Cella> Celle { get; set; } = new List<ReportPdf_Cella>();

        public ReportPdf_Riga() { }
        public ReportPdf_Riga(ReportPdf_TipoRiga tipo, params ReportPdf_Cella[] celle)
        {
            Tipo = tipo;
            Celle = celle.ToList();
        }
        public ReportPdf_Riga(params string[] testi) : this(ReportPdf_TipoRiga.Normale, testi.Select(t => new ReportPdf_Cella(t)).ToArray()) { }
    }

    public class ReportPdf_Cella
    {
        // piu' righe separate da '\n'
        public string Testo { get; set; } = string.Empty;
        // colori "#RRGGBB" (null = predefiniti della riga)
        public string? Sfondo { get; set; }
        public string? Colore { get; set; }
        // celle a destra unite a questa (come MergeRight di MigraDoc)
        public int Unisci { get; set; }
        // testo secondario (grigio)
        public bool Tenue { get; set; }

        public ReportPdf_Cella() { }
        public ReportPdf_Cella(string testo, int unisci = 0)
        {
            Testo = testo;
            Unisci = unisci;
        }
    }
}
