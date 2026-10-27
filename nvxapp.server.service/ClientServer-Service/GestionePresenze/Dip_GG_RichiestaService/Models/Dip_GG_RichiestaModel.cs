using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models
{




    public class Dip_GG_RichiestaModel : HashModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }

        [HashField]
        public string Data { get; set; } = string.Empty;
        [HashField]
        public string DataA { get; set; } = string.Empty;

        [HashField]
        public TipoRichiesta RichiestaTipo { get; set; }
        // Campo per oggetto JSON
        [HashField]
        public required string Dati { get; set; } = string.Empty;

        [HashField]
        public StatoRichiesta RichiestaStato { get; set; }
        [HashField]
        public List<Dip_GG_Richiesta_Stato_Cronology>? RichiestaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();
        [HashField]
        public StatoRichiesta? RevocaStato { get; set; }
        [HashField]
        public List<Dip_GG_Richiesta_Stato_Cronology>? RevocaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();

        // Campo per oggetto JSON
        [HashField]
        public string? CronologiaApprovazione { get; set; }

    }

    public class Dip_GG_Richiesta_Body_Timbratura
    {
        public required string hhmm { get; set; } = string.Empty;
        public required int IdAz_SubCommessaAttivita { get; set; }
    }
    public class Dip_GG_Richiesta_Body_Giustificativo
    {
        public required string hhmm { get; set; } = string.Empty;
        public required bool AllDay { get; set; } = false;
        public required int IdPar_Giustificativi { get; set; } = 0;
    }
    /*
     Nota spese: la richiesta e' il contenitore delle spese (dettagli). All'invio arrivano spese e allegati;
     salvati i dettagli (Dip_GG_NotaSpesa e Dip_GG_NotaSpesa_Allegato), in Dati resta solo il riepilogo.
    */
    public class Dip_GG_Richiesta_Body_NotaSpesa
    {
        public string Descrizione { get; set; } = string.Empty;
        public decimal Totale { get; set; }
        public int NumeroSpese { get; set; }

        /// <summary>solo nell'invio: spese con i loro allegati</summary>
        public List<Dip_GG_Richiesta_Body_NotaSpesa_Spesa>? Spese { get; set; }
    }

    public class Dip_GG_Richiesta_Body_NotaSpesa_Spesa
    {
        /// <summary>solo nella modifica: spesa gia' salvata da aggiornare (null = nuova spesa)</summary>
        public int? Id { get; set; }
        public DateTime Data { get; set; }
        public int IdPar_TipoSpesa { get; set; }
        /// <summary>per i tipi "rimborso km" lo ricalcola il server: km x rimborso per km dell'autovettura</summary>
        public decimal Importo { get; set; }
        public string? Descrizione { get; set; }
        /// <summary>solo per i tipi "rimborso km": km percorsi e autovettura del rapporto di lavoro</summary>
        public decimal? Km { get; set; }
        public int? IdDip_RapportoAutovetture { get; set; }
        public List<Dip_GG_Richiesta_Body_NotaSpesa_Allegato>? Allegati { get; set; }
    }

    public class Dip_GG_Richiesta_Body_NotaSpesa_Allegato
    {
        /// <summary>solo nella modifica: allegato gia' salvato da tenere (senza contenuto); null = nuovo allegato</summary>
        public int? Id { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        /// <summary>contenuto del file in base64</summary>
        public string Contenuto { get; set; } = string.Empty;
    }


    public class Dip_GG_Richiesta_GetAll4User_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Richiesta_GetAll4User_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }


    public class Dip_GG_Richiesta_GetAll4Admin_InModel
    {
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Richiesta_GetAll4Admin_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }




    public class Dip_GG_Richiesta_Send_InModel
    {
        public Boolean ExcludeRicalc { get; set; }
        public string? IdAspNetUsers { get; set; }
        public Dip_GG_RichiestaModel? Dip_GG_Richiesta { get; set; }
        public Boolean FromHR { get; set; }
    }
    // modifica di una nota spese non ancora approvata (dipendente che l'ha inviata)
    public class Dip_GG_Richiesta_NotaSpesaUpdate_InModel
    {
        public int IdDip_GG_Richiesta { get; set; }
        /// <summary>corpo della nota spese (Dip_GG_Richiesta_Body_NotaSpesa) con tutte le spese da tenere</summary>
        public string Dati { get; set; } = string.Empty;
    }
    public class Dip_GG_Richiesta_NotaSpesaUpdate_OutModel : ModelResult
    {
        public Dip_GG_RichiestaModel? Dip_GG_Richiesta { get; set; }
    }

    public class Dip_GG_Richiesta_Send_OutModel : ModelResult
    {
        public Dip_GG_Richiesta_Send_OutModel()
        {
            Dip_GG_Richiesta = new List<Dip_GG_RichiestaModel>();
            Dip_GG_Timbratura = new List<Dip_GG_TimbraturaModel>();
            Dip_GG_Giustificativi = new List<Dip_GG_GiustificativiModel>();
        }

        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
    }


    public class Dip_GG_Richiesta_SetState_InModel
    {
        public Boolean ExcludeRicalc { get; set; }
        public List<int> IdDip_GG_Richiesta { get; set; } = new List<int>();

        public StatoRichiesta RichiestaStato { get; set; }
        public Boolean FromHR { get; set; }

    }
    public class Dip_GG_Richiesta_SetState_OutModel : ModelResult
    {
        // dati modificati
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();

    }

    public class Dip_GG_Richiesta_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_GG_Richiesta_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }


    // contatori del menu "Richieste"
    public class Dip_GG_Richiesta_Count_InModel
    {
        // true = calcola anche le richieste da approvare (menu amministratore)
        public Boolean Admin { get; set; }
    }

    public class Dip_GG_Richiesta_Count_OutModel : ModelResult
    {
        // richieste dell'azienda che l'utente puo' approvare (o rifiutare) adesso
        public int DaApprovare { get; set; }
        // richieste dell'utente in attesa di approvazione
        public int InAttesa { get; set; }
    }


    public class Dip_GG_Richiesta_DeleteInModel
    {
        public Boolean ExcludeRicalc { get; set; }
        public int Id { get; set; }
    }
    public class Dip_GG_Richiesta_DeleteOutModel : ModelResult { }


    public class Dip_GG_RichiestaPutInModel
    {
        public Boolean ExcludeRicalc { get; set; }
        //public required int IdDip_RapportoLavoro { get; set; }
        public Dip_GG_RichiestaModel Dip_GG_Richiesta { get; set; } = new Dip_GG_RichiestaModel() { Id = 0, IdDip_RapportoLavoro=0, Dati=string.Empty };
    }
    public class Dip_GG_RichiestaPutOutModel : ModelResult
    {
        public Dip_GG_RichiestaModel Dip_GG_Richiesta { get; set; } = new Dip_GG_RichiestaModel() { Id = 0, IdDip_RapportoLavoro=0, Dati=string.Empty };
    }

}
