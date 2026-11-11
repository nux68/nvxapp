import { ModelResult } from '../../ModelsBase/model-result';

// valori uguali agli enum del server (finiscono nel database)
export enum CrmVisibilita { SoloPropri = 0, Tutti = 1 }
export enum CrmFaseTipo { Aperta = 0, Vinta = 1, Persa = 2 }
export enum CrmStatoCliente { Prospect = 0, Cliente = 1, ExCliente = 2, NonInteressato = 3 }
export enum CrmStatoLead { Nuovo = 0, Contattato = 1, Qualificato = 2, Convertito = 3, Scartato = 4 }
export enum CrmStatoAttivita { DaFare = 0, Fatta = 1, Annullata = 2 }
export enum CrmStatoOfferta { Bozza = 0, Inviata = 1, Accettata = 2, Rifiutata = 3, Scaduta = 4 }
export enum CrmTipoProdotto { Articolo = 0, Servizio = 1 }
// assistenza ai clienti (CRM-ASS)
export enum CrmStatoRichiesta { Nuova = 0, InCarico = 1, InAttesaCliente = 2, Risolta = 3, Chiusa = 4, Annullata = 5 }
export enum CrmPrioritaRichiesta { Bassa = 0, Normale = 1, Alta = 2, Urgente = 3 }
export enum CrmCanaleRichiesta { Telefono = 0, Email = 1, DiPersona = 2, Sito = 3, Altro = 4 }
export enum CrmParTabella { Fase = 0, TipoAttivita = 1, MotivoPerdita = 2, Fonte = 3, Prodotto = 4, Tag = 5 }

export class CrmEmptyInModel { }
export class CrmIdInModel { public id = 0; }
export class CrmIdOutModel extends ModelResult { public id = 0; }

export class CrmUtenteModel { public id = ''; public userName = ''; }
export class CrmPermessiModel { public powerAdmin = false; public admin = false; public vedeTutto = false; public idUtente = ''; }

// ---------------------------------------------------------------- configurazione
export class CrmCfgModel {
  public visibilita = CrmVisibilita.SoloPropri;
  public valuta = 'EUR';
  public minutiPreavviso = 30;
  public offertaPrefisso: string | null = null;
  public offertaGiorniValidita = 30;
  public offertaNote: string | null = null;
}

export class CrmParModel {
  public id = 0;
  public tabella = CrmParTabella.Fase;
  public descrizione = '';
  public ordine = 0;
  public attivo = true;
  public probabilita = 0;
  public tipo = CrmFaseTipo.Aperta;
  public icona: string | null = null;
  public colore: string | null = null;
  public durataMinuti = 30;
  public codice: string | null = null;
  public prezzo = 0;
  public um: string | null = null;
  public aliquotaIva = 22;
  public tipoProdotto = CrmTipoProdotto.Articolo;
}

export class CrmSetupGetOutModel extends ModelResult {
  public permessi = new CrmPermessiModel();
  public cfg = new CrmCfgModel();
  public fasi: CrmParModel[] = [];
  public tipiAttivita: CrmParModel[] = [];
  public motiviPerdita: CrmParModel[] = [];
  public fonti: CrmParModel[] = [];
  public prodotti: CrmParModel[] = [];
  public tag: CrmParModel[] = [];
  public utenti: CrmUtenteModel[] = [];
}
export class CrmCfgPutInModel { public cfg = new CrmCfgModel(); }
export class CrmCfgPutOutModel extends ModelResult { public cfg = new CrmCfgModel(); }
export class CrmParPutInModel { public par = new CrmParModel(); }
export class CrmParPutOutModel extends ModelResult { public par = new CrmParModel(); }
export class CrmParDeleteInModel { public tabella = CrmParTabella.Fase; public id = 0; }

export class CrmRiassegnaInModel {
  public idDa = '';
  public idA: string | null = null;
  public clienti = true;
  public opportunita = true;
  public lead = true;
  public attivitaDaFare = true;
}
export class CrmRiassegnaOutModel extends ModelResult { public clienti = 0; public opportunita = 0; public lead = 0; public attivita = 0; }

// ---------------------------------------------------------------- clienti
export class CrmClienteListInModel { public idResponsabile: string | null = null; public stato: CrmStatoCliente | null = null; public idTag: number | null = null; }
export class CrmClienteListModel {
  public id = 0;
  public descrizione = '';
  public tipo = 0;
  public attivo = true;
  public citta: string | null = null;
  public email: string | null = null;
  public telefono: string | null = null;
  public stato = CrmStatoCliente.Prospect;
  public idResponsabile: string | null = null;
  public responsabile: string | null = null;
  public tag: number[] = [];
  public opportunitaAperte = 0;
  public valoreAperto = 0;
  public ultimaAttivita: string | null = null;
  public prossimaAttivita: string | null = null;
}
export class CrmClienteListOutModel extends ModelResult { public clienti: CrmClienteListModel[] = []; }

export class CrmClienteModel {
  public id = 0;
  public descrizione = '';
  public tipo = 0;
  public attivo = true;
  public partitaIva: string | null = null;
  public codiceFiscale: string | null = null;
  public indirizzo: string | null = null;
  public cap: string | null = null;
  public citta: string | null = null;
  public provincia: string | null = null;
  public nazione: string | null = 'IT';
  public telefono: string | null = null;
  public email: string | null = null;
  public pec: string | null = null;
  public referente: string | null = null;
  public codiceDestinatario: string | null = null;
  public note: string | null = null;
  public stato = CrmStatoCliente.Prospect;
  public idCrm_Par_Fonte: number | null = null;
  public idResponsabile: string | null = null;
  public settore: string | null = null;
  public fatturatoStimato: number | null = null;
  public numeroDipendenti: number | null = null;
  public sito: string | null = null;
  public consensoMarketing = false;
  public dataConsenso: string | null = null;
  public tag: number[] = [];
}

export class CrmContattoModel {
  public id = 0;
  public idAz_Cliente = 0;
  public cognome: string | null = null;
  public nome: string | null = null;
  public ruolo: string | null = null;
  public telefono: string | null = null;
  public cellulare: string | null = null;
  public email: string | null = null;
  public principale = false;
  public note: string | null = null;
  public nominativo = '';
}

export class CrmClienteGetOutModel extends ModelResult {
  public cliente = new CrmClienteModel();
  public contatti: CrmContattoModel[] = [];
  public opportunita: CrmOpportunitaListModel[] = [];
  public attivita: CrmAttivitaListModel[] = [];
  public offerte: CrmOffertaListModel[] = [];
  public note: CrmNotaModel[] = [];
  public allegati: CrmAllegatoModel[] = [];
  public richieste: CrmRichiestaListModel[] = [];
  public modificabile = true;
}
export class CrmClientePutInModel { public cliente = new CrmClienteModel(); }
export class CrmClientePutOutModel extends ModelResult { public cliente = new CrmClienteModel(); }
export class CrmContattoPutInModel { public contatto = new CrmContattoModel(); }
export class CrmContattoPutOutModel extends ModelResult { public contatto = new CrmContattoModel(); }

// ---------------------------------------------------------------- lead
export class CrmLeadModel {
  public id = 0;
  public cognome: string | null = null;
  public nome: string | null = null;
  public azienda: string | null = null;
  public ruolo: string | null = null;
  public email: string | null = null;
  public telefono: string | null = null;
  public idCrm_Par_Fonte: number | null = null;
  public stato = CrmStatoLead.Nuovo;
  public idResponsabile: string | null = null;
  public responsabile: string | null = null;
  public note: string | null = null;
  public consensoMarketing = false;
  public dataConsenso: string | null = null;
  public idAz_Cliente: number | null = null;
  public cliente: string | null = null;
  public dataConversione: string | null = null;
  public creationDate: string | null = null;
}
export class CrmLeadListInModel { public ancheChiusi = false; }
export class CrmLeadListOutModel extends ModelResult { public lead: CrmLeadModel[] = []; }
export class CrmLeadGetOutModel extends ModelResult { public lead = new CrmLeadModel(); public attivita: CrmAttivitaListModel[] = []; public note: CrmNotaModel[] = []; }
export class CrmLeadPutInModel { public lead = new CrmLeadModel(); }
export class CrmLeadPutOutModel extends ModelResult { public lead = new CrmLeadModel(); }
export class CrmLeadConvertiInModel {
  public id = 0;
  public idAz_ClienteEsistente: number | null = null;
  public creaOpportunita = true;
  public titoloOpportunita: string | null = null;
  public valoreOpportunita = 0;
}
export class CrmLeadConvertiOutModel extends ModelResult { public idAz_Cliente = 0; public idCrm_Opportunita: number | null = null; }

// ---------------------------------------------------------------- opportunita'
export class CrmOpportunitaListInModel { public idAz_Cliente: number | null = null; public idResponsabile: string | null = null; public ancheChiuse = false; }
export class CrmOpportunitaListModel {
  public id = 0;
  public titolo = '';
  public idAz_Cliente = 0;
  public cliente = '';
  public idCrm_Par_Fase = 0;
  public fase = '';
  public faseTipo = CrmFaseTipo.Aperta;
  public valore = 0;
  public probabilita = 0;
  public dataChiusuraPrevista: string | null = null;
  public dataChiusura: string | null = null;
  public idResponsabile: string | null = null;
  public responsabile: string | null = null;
}
export class CrmOpportunitaListOutModel extends ModelResult { public opportunita: CrmOpportunitaListModel[] = []; }
export class CrmOpportunitaModel {
  public id = 0;
  public titolo = '';
  public idAz_Cliente = 0;
  public idAz_ClienteContatto: number | null = null;
  public idCrm_Par_Fase = 0;
  public valore = 0;
  public probabilita = 0;
  public dataChiusuraPrevista: string | null = null;
  public dataChiusura: string | null = null;
  public idResponsabile: string | null = null;
  public idCrm_Par_MotivoPerdita: number | null = null;
  public idCrm_Par_Fonte: number | null = null;
  public note: string | null = null;
  public idCommessaPresenze: number | null = null;
}
export class CrmStoricoModel { public data = ''; public utente: string | null = null; public faseDa: string | null = null; public faseA = ''; public valoreDa: number | null = null; public valoreA = 0; }
export class CrmOpportunitaGetOutModel extends ModelResult {
  public opportunita = new CrmOpportunitaModel();
  public cliente = '';
  public contatti: CrmContattoModel[] = [];
  public storico: CrmStoricoModel[] = [];
  public attivita: CrmAttivitaListModel[] = [];
  public offerte: CrmOffertaListModel[] = [];
  public note: CrmNotaModel[] = [];
  public allegati: CrmAllegatoModel[] = [];
}
export class CrmOpportunitaPutInModel { public opportunita = new CrmOpportunitaModel(); }
export class CrmOpportunitaPutOutModel extends ModelResult { public opportunita = new CrmOpportunitaModel(); }
export class CrmCambiaFaseInModel { public id = 0; public idCrm_Par_Fase = 0; public idCrm_Par_MotivoPerdita: number | null = null; }

// ---------------------------------------------------------------- attivita'
export class CrmAttivitaListInModel { public dal: string | null = null; public al: string | null = null; public soloMie = true; public soloDaFare = false; }
export class CrmAttivitaListModel {
  public id = 0;
  public idCrm_Par_TipoAttivita = 0;
  public tipo = '';
  public icona: string | null = null;
  public colore: string | null = null;
  public oggetto = '';
  public inizio = '';
  public fine: string | null = null;
  public stato = CrmStatoAttivita.DaFare;
  public idAssegnatario = '';
  public assegnatario: string | null = null;
  public idAz_Cliente: number | null = null;
  public cliente: string | null = null;
  public idCrm_Opportunita: number | null = null;
  public opportunita: string | null = null;
  public idCrm_Lead: number | null = null;
  public lead: string | null = null;
}
export class CrmAttivitaListOutModel extends ModelResult { public attivita: CrmAttivitaListModel[] = []; }
export class CrmAttivitaModel {
  public id = 0;
  public idCrm_Par_TipoAttivita = 0;
  public oggetto = '';
  public descrizione: string | null = null;
  public inizio = '';
  public fine: string | null = null;
  public idAssegnatario: string | null = null;
  public stato = CrmStatoAttivita.DaFare;
  public esito: string | null = null;
  public idAz_Cliente: number | null = null;
  public idAz_ClienteContatto: number | null = null;
  public idCrm_Opportunita: number | null = null;
  public idCrm_Lead: number | null = null;
}
export class CrmAttivitaGetOutModel extends ModelResult { public attivita = new CrmAttivitaModel(); public contatti: CrmContattoModel[] = []; public opportunitaCliente: CrmOpportunitaListModel[] = []; }
export class CrmAttivitaPutInModel { public attivita = new CrmAttivitaModel(); }
export class CrmAttivitaPutOutModel extends ModelResult { public attivita = new CrmAttivitaModel(); }
export class CrmAttivitaCompletaInModel { public id = 0; public esito: string | null = null; public annullata = false; }

// ---------------------------------------------------------------- offerte
export class CrmOffertaListInModel { public idAz_Cliente: number | null = null; public idCrm_Opportunita: number | null = null; }
export class CrmOffertaListModel {
  public id = 0;
  public numeroCompleto = '';
  public data = '';
  public validaFinoAl: string | null = null;
  public stato = CrmStatoOfferta.Bozza;
  public idAz_Cliente = 0;
  public cliente = '';
  public oggetto: string | null = null;
  public totale = 0;
  public idCrm_Opportunita: number | null = null;
  public dataEsito: string | null = null;
}
export class CrmOffertaListOutModel extends ModelResult { public offerte: CrmOffertaListModel[] = []; }
export class CrmOffertaRigaModel {
  public id = 0;
  public idCrm_Par_Prodotto: number | null = null;
  public descrizione = '';
  public quantita = 1;
  public um: string | null = null;
  public prezzo = 0;
  public scontoPerc = 0;
  public aliquotaIva = 22;
  public importo = 0;
}
export class CrmOffertaModel {
  public id = 0;
  public anno = 0;
  public numero = 0;
  public numeroCompleto = '';
  public data = '';
  public validaFinoAl: string | null = null;
  public stato = CrmStatoOfferta.Bozza;
  public idAz_Cliente = 0;
  public idAz_ClienteContatto: number | null = null;
  public idCrm_Opportunita: number | null = null;
  public oggetto: string | null = null;
  public note: string | null = null;
  public imponibile = 0;
  public iva = 0;
  public totale = 0;
  public dataInvio: string | null = null;
  public dataEsito: string | null = null;
  // predisposizione dell'ordine dell'Amministrazione (sola lettura)
  public idOrdineAmministrazione: number | null = null;
  public righe: CrmOffertaRigaModel[] = [];
}
export class CrmOffertaGetOutModel extends ModelResult {
  public offerta = new CrmOffertaModel();
  public contatti: CrmContattoModel[] = [];
  public opportunitaCliente: CrmOpportunitaListModel[] = [];
  public allegati: CrmAllegatoModel[] = [];
}
export class CrmOffertaPutInModel { public offerta = new CrmOffertaModel(); }
export class CrmOffertaPutOutModel extends ModelResult { public offerta = new CrmOffertaModel(); }
export class CrmOffertaInviaInModel { public id = 0; public email = ''; public messaggio: string | null = null; }
// PDF preparato da un job: il file arriva nella barra dei job
export class CrmOffertaPdfJobOutModel extends ModelResult { public jobId = ''; }

// ---------------------------------------------------------------- assistenza ai clienti (CRM-ASS)
export class CrmRichiestaListInModel { public idAz_Cliente: number | null = null; public soloAperte = false; public soloMie = false; }
export class CrmRichiestaListModel {
  public id = 0;
  public numeroCompleto = '';
  public idAz_Cliente = 0;
  public cliente = '';
  public oggetto = '';
  public priorita = CrmPrioritaRichiesta.Normale;
  public stato = CrmStatoRichiesta.Nuova;
  public idAssegnatario: string | null = null;
  public assegnatario: string | null = null;
  public dataApertura = '';
  public dataPresaInCarico: string | null = null;
  public dataRisoluzione: string | null = null;
  public orePresaInCarico: number | null = null;
  public oreRisoluzione: number | null = null;
}
export class CrmRichiestaListOutModel extends ModelResult { public richieste: CrmRichiestaListModel[] = []; }
export class CrmRichiestaModel {
  public id = 0;
  public numeroCompleto = '';
  public idAz_Cliente = 0;
  public idAz_ClienteContatto: number | null = null;
  public oggetto = '';
  public descrizione: string | null = null;
  public canale = CrmCanaleRichiesta.Telefono;
  public priorita = CrmPrioritaRichiesta.Normale;
  public stato = CrmStatoRichiesta.Nuova;
  public idAssegnatario: string | null = null;
  public dataApertura = '';
  public dataPresaInCarico: string | null = null;
  public dataRisoluzione: string | null = null;
  public dataChiusura: string | null = null;
  public orePresaInCarico: number | null = null;
  public oreRisoluzione: number | null = null;
  public soluzione: string | null = null;
  public idCrm_Opportunita: number | null = null;
}
export class CrmRichiestaMessaggioModel { public id = 0; public data = ''; public autore: string | null = null; public testo = ''; public sistema = false; }
export class CrmRichiestaGetOutModel extends ModelResult {
  public richiesta = new CrmRichiestaModel();
  public cliente = '';
  public contatti: CrmContattoModel[] = [];
  public opportunitaCliente: CrmOpportunitaListModel[] = [];
  public messaggi: CrmRichiestaMessaggioModel[] = [];
  public modificabile = true;
}
export class CrmRichiestaPutInModel { public richiesta = new CrmRichiestaModel(); }
export class CrmRichiestaPutOutModel extends ModelResult { public richiesta = new CrmRichiestaModel(); }
export class CrmRichiestaMessaggioPutInModel { public idCrm_Richiesta = 0; public testo = ''; }
export class CrmRichiestaMessaggioPutOutModel extends ModelResult { public messaggi: CrmRichiestaMessaggioModel[] = []; }

// ---------------------------------------------------------------- note e allegati
export class CrmNotaModel {
  public id = 0;
  public idAz_Cliente: number | null = null;
  public idCrm_Opportunita: number | null = null;
  public idCrm_Lead: number | null = null;
  public testo = '';
  public fissata = false;
  public autore: string | null = null;
  public data: string | null = null;
  public modificabile = true;
}
export class CrmNotaPutInModel { public nota = new CrmNotaModel(); }
export class CrmNotaPutOutModel extends ModelResult { public nota = new CrmNotaModel(); }
export class CrmAllegatoModel { public id = 0; public nomeFile = ''; public contentType = ''; public dimensione = 0; public autore: string | null = null; public data: string | null = null; }
export class CrmAllegatoUploadModel { public idAz_Cliente: number | null = null; public idCrm_Opportunita: number | null = null; public idCrm_Offerta: number | null = null; }
export class CrmAllegatoUploadOutModel extends ModelResult { public allegato = new CrmAllegatoModel(); }

// ---------------------------------------------------------------- report
export class CrmReportInModel { public dal = ''; public al = ''; }
export class CrmReportRigaModel { public chiave = ''; public etichetta = ''; public numero = 0; public valore = 0; public valorePesato = 0; }
export class CrmReportOutModel extends ModelResult {
  public pipelinePerFase: CrmReportRigaModel[] = [];
  public pipelinePerResponsabile: CrmReportRigaModel[] = [];
  public vintePerResponsabile: CrmReportRigaModel[] = [];
  public persePerMotivo: CrmReportRigaModel[] = [];
  public vinte = 0;
  public perse = 0;
  public valoreVinto = 0;
  public leadCreati = 0;
  public leadConvertiti = 0;
  public attivitaPerUtente: CrmReportRigaModel[] = [];
}
