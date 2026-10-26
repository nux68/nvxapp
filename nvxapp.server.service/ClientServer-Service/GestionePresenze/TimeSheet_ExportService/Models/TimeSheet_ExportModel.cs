using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models
{
    
    public class TimeSheet_ExportModel
    {

        public int Year { get; set; }
        public int Month { get; set; } = 0;
        public List<string> SelectedUserId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public int IdPar_ExportCau { get; set; }
        

    }

    public class TimeSheet_ExportInModel
    {
        public TimeSheet_ExportModel TimeSheet_Export { get; set; } = new TimeSheet_ExportModel();
    }
    public class TimeSheet_ExportOutModel : ModelResult
    {
        public TimeSheet_ExportModel TimeSheet_Export { get; set; } = new TimeSheet_ExportModel();

        public TimeSheet_ExportOutModel()
        {

        }
    }

    public class ExportRow
    {
        public string Cognome { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string CodiceCausale { get; set; } = string.Empty;
        public decimal Valore { get; set; }
    }


    // ── Cartellino PDF dell'utente collegato ─────────────────────────────

    /// <summary>Mese del cartellino: l'utente non si passa, e' sempre quello collegato.</summary>
    public class TimeSheet_ExportPdf_4User_Model
    {
        public int Year { get; set; }
        public int Month { get; set; }       // 1-12
    }

    public class TimeSheet_ExportPdf_4User_InModel
    {
        public TimeSheet_ExportPdf_4User_Model TimeSheet_ExportPdf { get; set; } = new TimeSheet_ExportPdf_4User_Model();
    }

    public class TimeSheet_ExportPdf_4User_OutModel : ModelResult
    {
        public string? JobId { get; set; }
    }


    // ── Cartellini PDF scelti dall'admin ─────────────────────────────────

    /// <summary>Mese e dipendenti scelti dall'admin: il server tiene solo quelli che l'admin puo' vedere.</summary>
    public class TimeSheet_ExportPdf_4Admin_Model
    {
        public int Year { get; set; }
        public int Month { get; set; }       // 1-12
        public List<string> SelectedUserId { get; set; } = new List<string>();
    }

    public class TimeSheet_ExportPdf_4Admin_InModel
    {
        public TimeSheet_ExportPdf_4Admin_Model TimeSheet_ExportPdf { get; set; } = new TimeSheet_ExportPdf_4Admin_Model();
    }

    public class TimeSheet_ExportPdf_4Admin_OutModel : ModelResult
    {
        public string? JobId { get; set; }
    }


    // ── Dati di un cartellino da stampare (un dipendente, un mese) ───────
    // Li riempie l'export (utente o, in futuro, admin) e li scrive TimeSheetPdfWriter.

    public class CartellinoPdf
    {
        public string Azienda { get; set; } = string.Empty;
        public string Dipendente { get; set; } = string.Empty;
        public int Anno { get; set; }
        public int Mese { get; set; }
        public string? StatoPeriodo { get; set; }
        public List<CartellinoPdf_Giorno> Giorni { get; set; } = new List<CartellinoPdf_Giorno>();
        public List<CartellinoPdf_Voce> Riepilogo { get; set; } = new List<CartellinoPdf_Voce>();
        public string? NoteHR { get; set; }
    }

    public class CartellinoPdf_Giorno
    {
        public DateTime Data { get; set; }
        public bool Festivo { get; set; }         // festivita' o fine settimana: riga evidenziata
        public string Timbrature { get; set; } = string.Empty;
        public string Giustificativi { get; set; } = string.Empty;
        public int? Min_Teo { get; set; }
        public int? Min_Lav { get; set; }
        public int? Min_InPiu { get; set; }       // >0 ore in piu', <0 ore mancanti
        public bool InPiuProvvisorie { get; set; }
    }

    public class CartellinoPdf_Voce
    {
        public string Etichetta { get; set; } = string.Empty;
        public int Minuti { get; set; }
        public bool Livello1 { get; set; }        // voce di dettaglio (rientrata)
        public bool Saldo { get; set; }           // mostra il segno + davanti ai positivi
        public string? Nota { get; set; }
    }


}
