import { ModelResult } from '../../ModelsBase/model-result';

// stessi valori degli enum del server (Doc_Categoria, Doc_Stato, Doc_LogAzione)
export enum DocCategoria { Personale = 0, Azienda = 1, Cliente = 2 }
export enum DocStato { Bozza = 0, Pubblicato = 1, Archiviato = 2 }

export class DocPermessiModel {
  public powerAdmin = false;
  public amministraArchivio = false;
  public vedePersonale = false;
  public caricaPersonale = false;
  public gestisceAbilitazioni = false;
  public dipendente = false;
}

export class DocCfgModel {
  public giorniCestino = 30;
  public versioniDaConservare = 0;
  public notificheEmail = true;
}

export class DocTipoModel {
  public id = 0;
  public codice = '';
  public descrizione = '';
  public categoria: DocCategoria = DocCategoria.Azienda;
  public anniConservazione: number | null = null;
  public haScadenza = false;
  public presaVisioneRichiesta = false;
  public haCompetenza = false;
  public ordine = 0;
  public active = true;
  public documenti = 0;
}

export class DocCartellaModel {
  public id = 0;
  public idDoc_Cartella: number | null = null;
  public descrizione = '';
  public percorso = '';
  public ordine = 0;
  public visibileUtenti = false;
  public diSistema = false;
  public documenti = 0;
}

export class DocDipendenteModel {
  public id = 0;
  public nominativo = '';
  public cognome: string | null = null;
  public nome: string | null = null;
  public codiceFiscale: string | null = null;
  public matricola: string | null = null;
  public email: string | null = null;
  public dataCessazione: string | null = null;
  public attivo = true;
  public haUtente = false;
}

export class DocAbilitazioneModel {
  public idAspNetUsers = '';
  public userName = '';
  public ruoli = '';
  public vedePersonale = false;
  public caricaPersonale = false;
}

export class DocDocumentoListModel {
  public id = 0;
  public titolo = '';
  public idDoc_Par_Tipo = 0;
  public tipo = '';
  public categoria: DocCategoria = DocCategoria.Azienda;
  public idDoc_Cartella: number | null = null;
  public cartella: string | null = null;
  public idDip_Anagrafica: number | null = null;
  public dipendente: string | null = null;
  public idAz_Cliente: number | null = null;
  public cliente: string | null = null;
  public dataDocumento: string | null = null;
  public anno: number | null = null;
  public mese: number | null = null;
  public dataScadenza: string | null = null;
  public stato: DocStato = DocStato.Pubblicato;
  public versioneCorrente = 1;
  public nomeFile = '';
  public dimensione = 0;
  public dataCaricamento: string | null = null;
  public nelCestino = false;
  public presaVisioneRichiesta = false;
  public dataPresaVisione: string | null = null;
  public nuovo = false;
  public oltreConservazione = false;
}

export class DocDocumentoEditModel {
  public id = 0;
  public idDoc_Par_Tipo: number | null = null;
  public idDoc_Cartella: number | null = null;
  public idDip_Anagrafica: number | null = null;
  public idAz_Cliente: number | null = null;
  public titolo = '';
  public descrizione: string | null = null;
  public dataDocumento: string | null = null;
  public anno: number | null = null;
  public mese: number | null = null;
  public dataScadenza: string | null = null;
  public stato: DocStato = DocStato.Pubblicato;
  public noteVersione: string | null = null;
}

export class DocVersioneModel {
  public id = 0;
  public numero = 0;
  public nomeFile = '';
  public contentType = '';
  public dimensione = 0;
  public hash = '';
  public data: string | null = null;
  public autore: string | null = null;
  public note: string | null = null;
}

export class DocDestinatarioModel {
  public id = 0;
  public nominativo = '';
  public presaVisioneRichiesta = false;
  public dataPrimaApertura: string | null = null;
  public dataPresaVisione: string | null = null;
  public dataDownload: string | null = null;
}

export class DocDettaglioModel {
  public documento = new DocDocumentoEditModel();
  public riepilogo = new DocDocumentoListModel();
  public versioni: DocVersioneModel[] = [];
  public destinatari: DocDestinatarioModel[] = [];
  public modificabile = false;
}

export class DocLogModel {
  public data = '';
  public userName: string | null = null;
  public azione = 0;
  public versione: number | null = null;
  public dettaglio: string | null = null;
}

export class DocClienteModel {
  public id = 0;
  public descrizione = '';
}

// ---------------------------------------------------------------- richieste e risposte
export class DocEmptyInModel { }
export class DocIdInModel { public id = 0; }
export class DocIdOutModel extends ModelResult { public id = 0; }
export class DocDeleteOutModel extends ModelResult { public deleted = false; }

export class DocSetupGetOutModel extends ModelResult {
  public permessi = new DocPermessiModel();
  public cfg = new DocCfgModel();
  public tipi: DocTipoModel[] = [];
  public cartelle: DocCartellaModel[] = [];
  public dipendenti: DocDipendenteModel[] = [];
}
export class DocCfgPutInModel { public cfg = new DocCfgModel(); }
export class DocCfgPutOutModel extends ModelResult { public cfg = new DocCfgModel(); }
export class DocTipoPutInModel { public tipo = new DocTipoModel(); }
export class DocTipoPutOutModel extends ModelResult { public tipo = new DocTipoModel(); }
export class DocCartellaPutInModel { public cartella = new DocCartellaModel(); }
export class DocCartellaPutOutModel extends ModelResult { public cartella = new DocCartellaModel(); }
export class DocDipendentePutInModel { public dipendente = new DocDipendenteModel(); }
export class DocDipendentePutOutModel extends ModelResult { public dipendente = new DocDipendenteModel(); }
export class DocAbilitazioneListOutModel extends ModelResult { public abilitazioni: DocAbilitazioneModel[] = []; }
export class DocAbilitazionePutInModel { public abilitazione = new DocAbilitazioneModel(); }
export class DocAbilitazionePutOutModel extends ModelResult { public abilitazione = new DocAbilitazioneModel(); }

export class DocDocumentoListInModel {
  public idDoc_Cartella: number | null = null;
  public idDoc_Par_Tipo: number | null = null;
  public idDip_Anagrafica: number | null = null;
  public idAz_Cliente: number | null = null;
  public categoria: DocCategoria | null = null;
  public testo: string | null = null;
  public anno: number | null = null;
  public cestino = false;
  public scadenzaEntroGiorni: number | null = null;
  public miei = false;
}
export class DocDocumentoListOutModel extends ModelResult {
  public documenti: DocDocumentoListModel[] = [];
  public troncato = false;
}
export class DocDocumentoGetOutModel extends ModelResult { public dettaglio = new DocDettaglioModel(); }
export class DocDocumentoPutInModel { public documento = new DocDocumentoEditModel(); }
export class DocDocumentoPutOutModel extends ModelResult { public dettaglio = new DocDettaglioModel(); }
export class DocSvuotaCestinoOutModel extends ModelResult { public eliminati = 0; }
export class DocLogListInModel { public idDoc_Documento: number | null = null; }
export class DocLogListOutModel extends ModelResult { public log: DocLogModel[] = []; }
export class DocClienteListOutModel extends ModelResult { public az_Cliente: DocClienteModel[] = []; }

// dimensione leggibile
export function docSize(bytes: number): string {
  if (bytes < 1024) return bytes + ' B';
  if (bytes < 1024 * 1024) return (bytes / 1024).toLocaleString(undefined, { maximumFractionDigits: 1 }) + ' KB';
  return (bytes / (1024 * 1024)).toLocaleString(undefined, { maximumFractionDigits: 1 }) + ' MB';
}

// data ISO del server -> yyyy-MM-dd per gli input di tipo date (e ritorno)
export function docDateInput(value: string | null | undefined): string | null {
  return value ? value.substring(0, 10) : null;
}
