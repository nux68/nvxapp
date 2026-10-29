using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models
{
    public class Dip_AnagraficaModel
    {
        public string UserName { get; set; } = string.Empty;
        public string IdAspNetUsers { get; set; } = string.Empty;
        public int Id { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        public List<RoleCode> RoleCode { get; set; } = new List<RoleCode>();

        // questi vengono usati per visualizzare i dati, ma le editazioni lavorano dievrsamente con propri ogegtti
        public List<Dip_RapportoLavoroModel> Dip_RapportoLavoro { get; set; } = new List<Dip_RapportoLavoroModel>();

    }

    /// <summary>altri dipendenti dell'azienda con la stessa matricola / lo stesso codice fiscale (cognome e nome), e l'errore da mostrare</summary>
    public class MatricolaCodiceFiscaleUsati
    {
        public string? DipendenteConMatricola { get; set; }
        public string? DipendenteConCodiceFiscale { get; set; }
        public string? Errore { get; set; }
    }

    public class Dip_Anagrafica_Check_InModel
    {
        public string? Matricola { get; set; }
        public string? CodiceFiscale { get; set; }
        /// <summary>dipendente in modifica (escluso dal controllo); vuoto per un nuovo dipendente</summary>
        public string? IdAspNetUsers { get; set; }
    }
    public class Dip_Anagrafica_Check_OutModel : ModelResult
    {
        public bool MatricolaExists { get; set; }
        public bool CodiceFiscaleExists { get; set; }
        public bool CodiceFiscaleNonValido { get; set; }
    }

    public class Dip_Anagrafica_GetAll_InModel
    {

    }

    public class Dip_Anagrafica_GetAll_OutModel : ModelResult
    {
        public List<Dip_AnagraficaModel> Dip_Anagrafica { get; set; } = new List<Dip_AnagraficaModel>();

        public Dip_Anagrafica_GetAll_OutModel()
        {

        }
    }




    public class Dip_Anagrafica4EditModel:Dip_AnagraficaModel
    {
        
        public string Descrizione { get; set; } = string.Empty;
        public int IdUserCompany { get; set; }
        public string? RoleId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        ////
        public Boolean MainUser { get; set; }
        // collegamento utente-azienda disabilitato (UserCompany.Disabled); null = non modificare
        public Boolean? Disabled { get; set; }
        public string? Mail { get; set; } = string.Empty;
        public string? Pw { get; set; } = string.Empty;

        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();

        /// <summary>Riporti (mese 0) per i 5 anni: anno corrente e i 4 precedenti.</summary>
        public List<Contatori_Riporto_Model> Dip_Contatori_Riporto { get; set; } = new List<Contatori_Riporto_Model>();

        /// <summary>Configurazione maturazione ore per ciascun giustificativo-contatore del rapporto di lavoro.</summary>
        public List<Dip_Rapporto_Giustificativi_MaturazioneModel> Dip_Maturazione { get; set; } = new List<Dip_Rapporto_Giustificativi_MaturazioneModel>();

        /// <summary>Competenze del dipendente: determinano le attivita' che puo' svolgere.</summary>
        public List<Dip_CompetenzaModel> Dip_Competenza { get; set; } = new List<Dip_CompetenzaModel>();

        /// <summary>
        /// Autovetture dei rapporti di lavoro (sezione "Spesa", rimborsi km).
        /// null = sezione non gestita dal client: al salvataggio le autovetture restano come sono.
        /// </summary>
        public List<Dip_RapportoAutovettureModel>? Dip_RapportoAutovetture { get; set; }

    }
    
    public class Dip_Anagrafica_Get_InModel
    {
        public string Id { get; set; }  = string.Empty; // = IdAspNetUsers
    }
    public class Dip_Anagrafica_Get_OutModel : ModelResult
    {
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }

    public class Dip_Anagrafica_Put_InModel
    {
        public string Id { get; set; } = string.Empty;// = IdAspNetUsers
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }
    public class Dip_Anagrafica_Put_OutModel : ModelResult
    {
        public string Id { get; set; } = string.Empty;// = IdAspNetUsers
        public Dip_Anagrafica4EditModel Dip_Anagrafica { get; set; } = new Dip_Anagrafica4EditModel();
    }


}
