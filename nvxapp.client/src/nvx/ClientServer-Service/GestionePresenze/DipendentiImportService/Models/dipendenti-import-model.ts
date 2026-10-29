import { ModelResult } from "../../../ModelsBase/model-result";

/** dipendente letto dal file: i dati del primo step del wizard di inserimento */
export class DipendenteImportModel {
  public cognome: string = '';
  public nome: string = '';
  public userName: string = '';
  public mail: string = '';
  /** data di assunzione "YYYY-MM-DDT00:00:00", vuota se il file non la riporta */
  public dataAss: string = '';
  /** username o email gia' usati da un altro utente */
  public userNameExists: boolean = false;
  public mailExists: boolean = false;
}

/** dimensione massima del file da importare (come sul server) */
export const DIPENDENTI_IMPORT_MAX_BYTE = 5 * 1024 * 1024;

export class DipendentiImport_EstraiInModel {
  public nomeFile: string = '';
  /** contenuto del file in base64 */
  public contenuto: string = '';
  /** indicazioni dell'amministratore per il LLM (facoltative) */
  public note: string | null = null;
}

/** dipendente da inserire, con i dati corretti nel dialog */
export class DipendenteImportDaInserireModel {
  public cognome: string = '';
  public nome: string = '';
  public userName: string = '';
  public mail: string = '';
  public roles: string[] = [];
  /** "YYYY-MM-DDT00:00:00"; null = oggi */
  public dataAss: string | null = null;
  public idPar_ProfiloOrario: number = 0;
}

export class DipendentiImport_EseguiInModel {
  /** password iniziale, uguale per tutti */
  public password: string = '';
  public dipendenti: DipendenteImportDaInserireModel[] = [];
}

export class DipendentiImport_EseguiOutModel extends ModelResult {
  /** job avviato: avanzamento e fine arrivano con le notifiche LongJobProgress */
  public jobId: string = '';
}

export class DipendentiImport_EstraiOutModel extends ModelResult {
  public dipendenti: DipendenteImportModel[] = [];
  /** file troppo lungo: e' stata letta solo la prima parte */
  public testoTroncato: boolean = false;
}
