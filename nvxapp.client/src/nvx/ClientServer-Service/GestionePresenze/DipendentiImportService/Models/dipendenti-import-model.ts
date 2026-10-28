import { ModelResult } from "../../../ModelsBase/model-result";

/** dipendente letto dal file: i dati del primo step del wizard di inserimento */
export class DipendenteImportModel {
  public cognome: string = '';
  public nome: string = '';
  public userName: string = '';
  public mail: string = '';
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
}

export class DipendentiImport_EstraiOutModel extends ModelResult {
  public dipendenti: DipendenteImportModel[] = [];
  /** file troppo lungo: e' stata letta solo la prima parte */
  public testoTroncato: boolean = false;
}
