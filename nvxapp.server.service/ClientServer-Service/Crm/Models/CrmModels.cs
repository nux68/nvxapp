using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Crm.Models
{
    // ------------------------------------------------------------------------------------------------ comuni
    public class Crm_EmptyInModel { }
    public class Crm_IdInModel { public int Id { get; set; } }
    public class Crm_IdOutModel : ModelResult { public int Id { get; set; } }

    // utente dell'azienda proponibile come responsabile / assegnatario
    public class Crm_UtenteModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    // cosa puo' fare l'utente corrente (il server lo ricontrolla in ogni servizio)
    public class Crm_PermessiModel
    {
        public bool PowerAdmin { get; set; }
        public bool Admin { get; set; }
        public bool VedeTutto { get; set; }
        public string IdUtente { get; set; } = string.Empty;
    }

    // ------------------------------------------------------------------------------------------------ configurazione
    public class Crm_CfgModel
    {
        public Crm_Visibilita Visibilita { get; set; }
        public string Valuta { get; set; } = "EUR";
        public int MinutiPreavviso { get; set; }
        public string? OffertaPrefisso { get; set; }
        public int OffertaGiorniValidita { get; set; }
        public string? OffertaNote { get; set; }
    }

    // riga di una tabella di configurazione (Crm_Par_*): i campi non usati da una tabella restano vuoti
    public class Crm_ParModel
    {
        public int Id { get; set; }
        public Crm_ParTabella Tabella { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int Ordine { get; set; }
        public bool Attivo { get; set; } = true;
        // fase
        public int Probabilita { get; set; }
        public Crm_FaseTipo Tipo { get; set; }
        // tipo attivita' e tag
        public string? Icona { get; set; }
        public string? Colore { get; set; }
        public int DurataMinuti { get; set; }
        // prodotto
        public string? Codice { get; set; }
        public decimal Prezzo { get; set; }
        public string? Um { get; set; }
        public decimal AliquotaIva { get; set; }
        public Crm_TipoProdotto TipoProdotto { get; set; }
    }

    public class Crm_SetupGetOutModel : ModelResult
    {
        public Crm_PermessiModel Permessi { get; set; } = new();
        public Crm_CfgModel Cfg { get; set; } = new();
        public List<Crm_ParModel> Fasi { get; set; } = new();
        public List<Crm_ParModel> TipiAttivita { get; set; } = new();
        public List<Crm_ParModel> MotiviPerdita { get; set; } = new();
        public List<Crm_ParModel> Fonti { get; set; } = new();
        public List<Crm_ParModel> Prodotti { get; set; } = new();
        public List<Crm_ParModel> Tag { get; set; } = new();
        public List<Crm_UtenteModel> Utenti { get; set; } = new();
    }

    public class Crm_CfgPutInModel { public Crm_CfgModel Cfg { get; set; } = new(); }
    public class Crm_CfgPutOutModel : ModelResult { public Crm_CfgModel Cfg { get; set; } = new(); }
    public class Crm_ParPutInModel { public Crm_ParModel Par { get; set; } = new(); }
    public class Crm_ParPutOutModel : ModelResult { public Crm_ParModel Par { get; set; } = new(); }
    public class Crm_ParDeleteInModel { public Crm_ParTabella Tabella { get; set; } public int Id { get; set; } }

    // riassegnazione in blocco (es. dimissioni di un commerciale)
    public class Crm_RiassegnaInModel
    {
        public string IdDa { get; set; } = string.Empty;
        public string? IdA { get; set; }
        public bool Clienti { get; set; } = true;
        public bool Opportunita { get; set; } = true;
        public bool Lead { get; set; } = true;
        public bool AttivitaDaFare { get; set; } = true;
    }
    public class Crm_RiassegnaOutModel : ModelResult
    {
        public int Clienti { get; set; }
        public int Opportunita { get; set; }
        public int Lead { get; set; }
        public int Attivita { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ clienti
    public class Crm_ClienteListInModel
    {
        public string? IdResponsabile { get; set; }
        public Crm_StatoCliente? Stato { get; set; }
        public int? IdTag { get; set; }
    }

    public class Crm_ClienteListModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public bool Attivo { get; set; }
        public string? Citta { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public Crm_StatoCliente Stato { get; set; }
        public string? IdResponsabile { get; set; }
        public string? Responsabile { get; set; }
        public List<int> Tag { get; set; } = new();
        public int OpportunitaAperte { get; set; }
        public decimal ValoreAperto { get; set; }
        public DateTime? UltimaAttivita { get; set; }
        public DateTime? ProssimaAttivita { get; set; }
    }
    public class Crm_ClienteListOutModel : ModelResult { public List<Crm_ClienteListModel> Clienti { get; set; } = new(); }

    // anagrafica comune + dati del CRM
    public class Crm_ClienteModel
    {
        public int Id { get; set; }
        // Az_Cliente
        public string Descrizione { get; set; } = string.Empty;
        public int Tipo { get; set; }
        public bool Attivo { get; set; } = true;
        public string? PartitaIva { get; set; }
        public string? CodiceFiscale { get; set; }
        public string? Indirizzo { get; set; }
        public string? Cap { get; set; }
        public string? Citta { get; set; }
        public string? Provincia { get; set; }
        public string? Nazione { get; set; } = "IT";
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Pec { get; set; }
        public string? Referente { get; set; }
        public string? CodiceDestinatario { get; set; }
        public string? Note { get; set; }
        // Crm_ClienteDati
        public Crm_StatoCliente Stato { get; set; }
        public int? IdCrm_Par_Fonte { get; set; }
        public string? IdResponsabile { get; set; }
        public string? Settore { get; set; }
        public decimal? FatturatoStimato { get; set; }
        public int? NumeroDipendenti { get; set; }
        public string? Sito { get; set; }
        public bool ConsensoMarketing { get; set; }
        public DateTime? DataConsenso { get; set; }
        public List<int> Tag { get; set; } = new();
    }

    public class Crm_ContattoModel
    {
        public int Id { get; set; }
        public int IdAz_Cliente { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Ruolo { get; set; }
        public string? Telefono { get; set; }
        public string? Cellulare { get; set; }
        public string? Email { get; set; }
        public bool Principale { get; set; }
        public string? Note { get; set; }
        public string Nominativo => string.Join(" ", new[] { Cognome, Nome }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    public class Crm_ClienteGetOutModel : ModelResult
    {
        public Crm_ClienteModel Cliente { get; set; } = new();
        public List<Crm_ContattoModel> Contatti { get; set; } = new();
        public List<Crm_OpportunitaListModel> Opportunita { get; set; } = new();
        public List<Crm_AttivitaListModel> Attivita { get; set; } = new();
        public List<Crm_OffertaListModel> Offerte { get; set; } = new();
        public List<Crm_NotaModel> Note { get; set; } = new();
        public List<Crm_AllegatoModel> Allegati { get; set; } = new();
        // richieste di assistenza del cliente (CRM-ASS.2)
        public List<Crm_RichiestaListModel> Richieste { get; set; } = new();
        // il cliente si puo' modificare (visibilita' e ruolo)
        public bool Modificabile { get; set; }
    }
    public class Crm_ClientePutInModel { public Crm_ClienteModel Cliente { get; set; } = new(); }
    public class Crm_ClientePutOutModel : ModelResult { public Crm_ClienteModel Cliente { get; set; } = new(); }
    public class Crm_ContattoPutInModel { public Crm_ContattoModel Contatto { get; set; } = new(); }
    public class Crm_ContattoPutOutModel : ModelResult { public Crm_ContattoModel Contatto { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ lead
    public class Crm_LeadModel
    {
        public int Id { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Azienda { get; set; }
        public string? Ruolo { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public int? IdCrm_Par_Fonte { get; set; }
        public Crm_StatoLead Stato { get; set; }
        public string? IdResponsabile { get; set; }
        public string? Responsabile { get; set; }
        public string? Note { get; set; }
        public bool ConsensoMarketing { get; set; }
        public DateTime? DataConsenso { get; set; }
        public int? IdAz_Cliente { get; set; }
        public string? Cliente { get; set; }
        public DateTime? DataConversione { get; set; }
        public DateTime? CreationDate { get; set; }
    }
    public class Crm_LeadListInModel { public bool AncheChiusi { get; set; } }
    public class Crm_LeadListOutModel : ModelResult { public List<Crm_LeadModel> Lead { get; set; } = new(); }
    public class Crm_LeadGetOutModel : ModelResult { public Crm_LeadModel Lead { get; set; } = new(); public List<Crm_AttivitaListModel> Attivita { get; set; } = new(); public List<Crm_NotaModel> Note { get; set; } = new(); }
    public class Crm_LeadPutInModel { public Crm_LeadModel Lead { get; set; } = new(); }
    public class Crm_LeadPutOutModel : ModelResult { public Crm_LeadModel Lead { get; set; } = new(); }

    // conversione: cliente nuovo (dai dati del lead) o esistente, contatto e, se indicata, opportunita'
    public class Crm_LeadConvertiInModel
    {
        public int Id { get; set; }
        public int? IdAz_ClienteEsistente { get; set; }
        public bool CreaOpportunita { get; set; }
        public string? TitoloOpportunita { get; set; }
        public decimal ValoreOpportunita { get; set; }
    }
    public class Crm_LeadConvertiOutModel : ModelResult
    {
        public int IdAz_Cliente { get; set; }
        public int? IdCrm_Opportunita { get; set; }
    }

    // ------------------------------------------------------------------------------------------------ opportunita'
    public class Crm_OpportunitaListInModel
    {
        public int? IdAz_Cliente { get; set; }
        public string? IdResponsabile { get; set; }
        public bool AncheChiuse { get; set; }
    }

    public class Crm_OpportunitaListModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public int IdAz_Cliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public int IdCrm_Par_Fase { get; set; }
        public string Fase { get; set; } = string.Empty;
        public Crm_FaseTipo FaseTipo { get; set; }
        public decimal Valore { get; set; }
        public int Probabilita { get; set; }
        public DateTime? DataChiusuraPrevista { get; set; }
        public DateTime? DataChiusura { get; set; }
        public string? IdResponsabile { get; set; }
        public string? Responsabile { get; set; }
    }
    public class Crm_OpportunitaListOutModel : ModelResult { public List<Crm_OpportunitaListModel> Opportunita { get; set; } = new(); }

    public class Crm_OpportunitaModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public int IdAz_Cliente { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public int IdCrm_Par_Fase { get; set; }
        public decimal Valore { get; set; }
        public int Probabilita { get; set; }
        public DateTime? DataChiusuraPrevista { get; set; }
        public DateTime? DataChiusura { get; set; }
        public string? IdResponsabile { get; set; }
        public int? IdCrm_Par_MotivoPerdita { get; set; }
        public int? IdCrm_Par_Fonte { get; set; }
        public string? Note { get; set; }
        public int? IdCommessaPresenze { get; set; }
    }

    public class Crm_StoricoModel
    {
        public DateTime Data { get; set; }
        public string? Utente { get; set; }
        public string? FaseDa { get; set; }
        public string FaseA { get; set; } = string.Empty;
        public decimal? ValoreDa { get; set; }
        public decimal ValoreA { get; set; }
    }

    public class Crm_OpportunitaGetOutModel : ModelResult
    {
        public Crm_OpportunitaModel Opportunita { get; set; } = new();
        public string Cliente { get; set; } = string.Empty;
        public List<Crm_ContattoModel> Contatti { get; set; } = new();
        public List<Crm_StoricoModel> Storico { get; set; } = new();
        public List<Crm_AttivitaListModel> Attivita { get; set; } = new();
        public List<Crm_OffertaListModel> Offerte { get; set; } = new();
        public List<Crm_NotaModel> Note { get; set; } = new();
        public List<Crm_AllegatoModel> Allegati { get; set; } = new();
    }
    public class Crm_OpportunitaPutInModel { public Crm_OpportunitaModel Opportunita { get; set; } = new(); }
    public class Crm_OpportunitaPutOutModel : ModelResult { public Crm_OpportunitaModel Opportunita { get; set; } = new(); }
    public class Crm_CambiaFaseInModel { public int Id { get; set; } public int IdCrm_Par_Fase { get; set; } public int? IdCrm_Par_MotivoPerdita { get; set; } }

    // ------------------------------------------------------------------------------------------------ attivita'
    public class Crm_AttivitaListInModel
    {
        public DateTime? Dal { get; set; }
        public DateTime? Al { get; set; }
        // solo le mie (assegnate all'utente corrente)
        public bool SoloMie { get; set; } = true;
        public bool SoloDaFare { get; set; }
    }

    public class Crm_AttivitaListModel
    {
        public int Id { get; set; }
        public int IdCrm_Par_TipoAttivita { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Icona { get; set; }
        public string? Colore { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }
        public Crm_StatoAttivita Stato { get; set; }
        public string IdAssegnatario { get; set; } = string.Empty;
        public string? Assegnatario { get; set; }
        public int? IdAz_Cliente { get; set; }
        public string? Cliente { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public string? Opportunita { get; set; }
        public int? IdCrm_Lead { get; set; }
        public string? Lead { get; set; }
    }
    public class Crm_AttivitaListOutModel : ModelResult { public List<Crm_AttivitaListModel> Attivita { get; set; } = new(); }

    public class Crm_AttivitaModel
    {
        public int Id { get; set; }
        public int IdCrm_Par_TipoAttivita { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime? Fine { get; set; }
        public string? IdAssegnatario { get; set; }
        public Crm_StatoAttivita Stato { get; set; }
        public string? Esito { get; set; }
        public int? IdAz_Cliente { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public int? IdCrm_Lead { get; set; }
    }
    public class Crm_AttivitaGetOutModel : ModelResult
    {
        public Crm_AttivitaModel Attivita { get; set; } = new();
        public List<Crm_ContattoModel> Contatti { get; set; } = new();
        public List<Crm_OpportunitaListModel> OpportunitaCliente { get; set; } = new();
    }
    public class Crm_AttivitaPutInModel { public Crm_AttivitaModel Attivita { get; set; } = new(); }
    public class Crm_AttivitaPutOutModel : ModelResult { public Crm_AttivitaModel Attivita { get; set; } = new(); }
    public class Crm_AttivitaCompletaInModel { public int Id { get; set; } public string? Esito { get; set; } public bool Annullata { get; set; } }

    // ------------------------------------------------------------------------------------------------ offerte
    public class Crm_OffertaListInModel { public int? IdAz_Cliente { get; set; } public int? IdCrm_Opportunita { get; set; } }

    public class Crm_OffertaListModel
    {
        public int Id { get; set; }
        public string NumeroCompleto { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public DateTime? ValidaFinoAl { get; set; }
        public Crm_StatoOfferta Stato { get; set; }
        public int IdAz_Cliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? Oggetto { get; set; }
        public decimal Totale { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public DateTime? DataEsito { get; set; }
    }
    public class Crm_OffertaListOutModel : ModelResult { public List<Crm_OffertaListModel> Offerte { get; set; } = new(); }

    public class Crm_OffertaRigaModel
    {
        public int Id { get; set; }
        public int? IdCrm_Par_Prodotto { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public decimal Quantita { get; set; } = 1;
        public string? Um { get; set; }
        public decimal Prezzo { get; set; }
        public decimal ScontoPerc { get; set; }
        public decimal AliquotaIva { get; set; }
        public decimal Importo { get; set; }
    }

    public class Crm_OffertaModel
    {
        public int Id { get; set; }
        public int Anno { get; set; }
        public int Numero { get; set; }
        public string NumeroCompleto { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public DateTime? ValidaFinoAl { get; set; }
        public Crm_StatoOfferta Stato { get; set; }
        public int IdAz_Cliente { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public string? Oggetto { get; set; }
        public string? Note { get; set; }
        public decimal Imponibile { get; set; }
        public decimal Iva { get; set; }
        public decimal Totale { get; set; }
        public DateTime? DataInvio { get; set; }
        public DateTime? DataEsito { get; set; }
        // predisposizione dell'ordine dell'Amministrazione (sola lettura)
        public int? IdOrdineAmministrazione { get; set; }
        public List<Crm_OffertaRigaModel> Righe { get; set; } = new();
    }
    public class Crm_OffertaGetOutModel : ModelResult
    {
        public Crm_OffertaModel Offerta { get; set; } = new();
        public List<Crm_ContattoModel> Contatti { get; set; } = new();
        public List<Crm_OpportunitaListModel> OpportunitaCliente { get; set; } = new();
        public List<Crm_AllegatoModel> Allegati { get; set; } = new();
    }
    public class Crm_OffertaPutInModel { public Crm_OffertaModel Offerta { get; set; } = new(); }
    public class Crm_OffertaPutOutModel : ModelResult { public Crm_OffertaModel Offerta { get; set; } = new(); }
    public class Crm_OffertaInviaInModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Messaggio { get; set; }
    }
    // PDF dell'offerta preparato da un job: il file arriva nella barra dei job
    public class Crm_OffertaPdfJobOutModel : ModelResult { public string JobId { get; set; } = string.Empty; }

    // ------------------------------------------------------------------------------------------------ assistenza (CRM-ASS)
    public class Crm_RichiestaListInModel
    {
        public int? IdAz_Cliente { get; set; }
        // solo le aperte (Nuova, In carico, In attesa del cliente)
        public bool SoloAperte { get; set; }
        // solo quelle assegnate all'utente corrente
        public bool SoloMie { get; set; }
    }

    public class Crm_RichiestaListModel
    {
        public int Id { get; set; }
        public string NumeroCompleto { get; set; } = string.Empty;
        public int IdAz_Cliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Oggetto { get; set; } = string.Empty;
        public Crm_PrioritaRichiesta Priorita { get; set; }
        public Crm_StatoRichiesta Stato { get; set; }
        public string? IdAssegnatario { get; set; }
        public string? Assegnatario { get; set; }
        public DateTime DataApertura { get; set; }
        public DateTime? DataPresaInCarico { get; set; }
        public DateTime? DataRisoluzione { get; set; }
        // ore dall'apertura alla presa in carico e alla risoluzione
        public double? OrePresaInCarico { get; set; }
        public double? OreRisoluzione { get; set; }
    }
    public class Crm_RichiestaListOutModel : ModelResult { public List<Crm_RichiestaListModel> Richieste { get; set; } = new(); }

    public class Crm_RichiestaModel
    {
        public int Id { get; set; }
        public string NumeroCompleto { get; set; } = string.Empty;
        public int IdAz_Cliente { get; set; }
        public int? IdAz_ClienteContatto { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public string? Descrizione { get; set; }
        public Crm_CanaleRichiesta Canale { get; set; }
        public Crm_PrioritaRichiesta Priorita { get; set; } = Crm_PrioritaRichiesta.Normale;
        public Crm_StatoRichiesta Stato { get; set; }
        public string? IdAssegnatario { get; set; }
        public DateTime DataApertura { get; set; }
        public DateTime? DataPresaInCarico { get; set; }
        public DateTime? DataRisoluzione { get; set; }
        public DateTime? DataChiusura { get; set; }
        public double? OrePresaInCarico { get; set; }
        public double? OreRisoluzione { get; set; }
        public string? Soluzione { get; set; }
        public int? IdCrm_Opportunita { get; set; }
    }

    public class Crm_RichiestaMessaggioModel
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string? Autore { get; set; }
        public string Testo { get; set; } = string.Empty;
        public bool Sistema { get; set; }
    }

    public class Crm_RichiestaGetOutModel : ModelResult
    {
        public Crm_RichiestaModel Richiesta { get; set; } = new();
        public string Cliente { get; set; } = string.Empty;
        public List<Crm_ContattoModel> Contatti { get; set; } = new();
        public List<Crm_OpportunitaListModel> OpportunitaCliente { get; set; } = new();
        public List<Crm_RichiestaMessaggioModel> Messaggi { get; set; } = new();
        // l'utente puo' lavorarla (stato, soluzione); riassegnarla ad altri e cancellarla solo gli amministratori
        public bool Modificabile { get; set; }
    }
    public class Crm_RichiestaPutInModel { public Crm_RichiestaModel Richiesta { get; set; } = new(); }
    public class Crm_RichiestaPutOutModel : ModelResult { public Crm_RichiestaModel Richiesta { get; set; } = new(); }
    public class Crm_RichiestaMessaggioPutInModel { public int IdCrm_Richiesta { get; set; } public string Testo { get; set; } = string.Empty; }
    public class Crm_RichiestaMessaggioPutOutModel : ModelResult { public List<Crm_RichiestaMessaggioModel> Messaggi { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ note e allegati
    public class Crm_NotaModel
    {
        public int Id { get; set; }
        public int? IdAz_Cliente { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public int? IdCrm_Lead { get; set; }
        public string Testo { get; set; } = string.Empty;
        public bool Fissata { get; set; }
        public string? Autore { get; set; }
        public DateTime? Data { get; set; }
        public bool Modificabile { get; set; }
    }
    public class Crm_NotaPutInModel { public Crm_NotaModel Nota { get; set; } = new(); }
    public class Crm_NotaPutOutModel : ModelResult { public Crm_NotaModel Nota { get; set; } = new(); }

    public class Crm_AllegatoModel
    {
        public int Id { get; set; }
        public string NomeFile { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Dimensione { get; set; }
        public string? Autore { get; set; }
        public DateTime? Data { get; set; }
    }
    // a cosa si allega (uno dei tre)
    public class Crm_AllegatoUploadModel
    {
        public int? IdAz_Cliente { get; set; }
        public int? IdCrm_Opportunita { get; set; }
        public int? IdCrm_Offerta { get; set; }
    }
    public class Crm_AllegatoUploadOutModel : ModelResult { public Crm_AllegatoModel Allegato { get; set; } = new(); }

    // file da scaricare (non e' un GenericResult: il controller lo restituisce come file)
    public sealed class Crm_FileDownload
    {
        public Stream Content { get; init; } = Stream.Null;
        public string NomeFile { get; init; } = string.Empty;
        public string ContentType { get; init; } = "application/octet-stream";
    }

    // ------------------------------------------------------------------------------------------------ report
    public class Crm_ReportInModel
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Crm_ReportRigaModel
    {
        public string Chiave { get; set; } = string.Empty;
        public string Etichetta { get; set; } = string.Empty;
        public int Numero { get; set; }
        public decimal Valore { get; set; }
        public decimal ValorePesato { get; set; }
    }

    public class Crm_ReportOutModel : ModelResult
    {
        // pipeline aperta (ad oggi) per fase e per responsabile
        public List<Crm_ReportRigaModel> PipelinePerFase { get; set; } = new();
        public List<Crm_ReportRigaModel> PipelinePerResponsabile { get; set; } = new();
        // opportunita' chiuse nel periodo
        public List<Crm_ReportRigaModel> VintePerResponsabile { get; set; } = new();
        public List<Crm_ReportRigaModel> PersePerMotivo { get; set; } = new();
        public int Vinte { get; set; }
        public int Perse { get; set; }
        public decimal ValoreVinto { get; set; }
        // tasso di conversione dei lead creati nel periodo
        public int LeadCreati { get; set; }
        public int LeadConvertiti { get; set; }
        // attivita' fatte nel periodo per utente
        public List<Crm_ReportRigaModel> AttivitaPerUtente { get; set; } = new();
    }
}
