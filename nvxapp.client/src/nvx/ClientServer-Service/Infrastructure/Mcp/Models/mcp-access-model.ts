import { ModelResult } from "../../../ModelsBase/model-result";


// Accesso di un'AI esterna tramite MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md):
// token personali, impostazione dell'azienda, registro delle chiamate.

/** token personale nell'elenco: il valore non c'e' mai (si vede una volta sola, alla creazione) */
export class McpTokenModel {
  public id = 0;
  public name = '';
  /** primi caratteri del token, per riconoscerlo */
  public tokenPrefix = '';
  /** utente con cui lavora il client AI */
  public userName = '';
  /** azienda su cui lavora; vuoto = nessuna (centro, studio, superuser) */
  public company = '';
  /** vero se il token lavora come un utente diverso da chi lo ha creato (impersonificazione) */
  public impersonated = false;
  public language = 'it';
  public createdAt: string | null = null;
  public expiresAt: string | null = null;
  public revokedAt: string | null = null;
  public lastUsedAt: string | null = null;
  /** "active", "expired" o "revoked" */
  public state = '';
}

/** stato dell'accesso AI per l'utente corrente */
export class McpStatusModel {
  /** endpoint MCP presente in questa installazione */
  public serverEnabled = false;
  /** indirizzo da indicare al client AI */
  public endpointUrl = '';
  public hasCompany = false;
  /** accesso AI attivo per l'azienda */
  public companyEnabled = false;
  /** livello massimo degli strumenti per l'azienda: 1 lettura, 2 modifiche personali, 3 modifiche amministrative */
  public companyMaxLevel = 0;
  /** livello massimo ammesso dall'installazione */
  public serverMaxLevel = 1;
  /** l'utente puo' cambiare l'impostazione dell'azienda */
  public canManageCompany = false;
  /** durata massima di un token, in giorni */
  public tokenMaxDays = 365;
}


export class McpTokenListInModel {
  /** anche i token revocati o scaduti */
  public showAll = false;
}

export class McpTokenListOutModel extends ModelResult {
  public mcpToken: McpTokenModel[] = [];
  public status: McpStatusModel = new McpStatusModel();
}


export class McpTokenCreateInModel {
  public name = '';
  /** durata in giorni */
  public days = 90;
  /** "it" o "en" */
  public language: string | null = null;
}

export class McpTokenCreateOutModel extends ModelResult {
  public mcpToken: McpTokenModel | null = null;
  /** valore del token: restituito solo qui, non e' piu' leggibile in seguito */
  public token = '';
  public endpointUrl = '';
}


export class McpTokenRevokeInModel {
  public id = 0;
}

export class McpTokenRevokeOutModel extends ModelResult {
}


export class McpCompanySettingModel {
  public enabled = false;
  /** 1 lettura, 2 modifiche personali, 3 modifiche amministrative */
  public maxLevel = 1;
}

export class McpCompanySettingGetInModel {
}

export class McpCompanySettingGetOutModel extends ModelResult {
  public mcpCompanySetting: McpCompanySettingModel = new McpCompanySettingModel();
  public status: McpStatusModel = new McpStatusModel();
}

export class McpCompanySettingPutInModel {
  public mcpCompanySetting: McpCompanySettingModel = new McpCompanySettingModel();
}

export class McpCompanySettingPutOutModel extends ModelResult {
  public mcpCompanySetting: McpCompanySettingModel = new McpCompanySettingModel();
}


export class McpCallLogModel {
  public id = 0;
  public data: string | null = null;
  public tokenName = '';
  /** utente che ha fatto il login (chi ha creato il token) */
  public userNameLogin = '';
  /** utente con cui ha lavorato il client AI */
  public userName = '';
  public tool = '';
  public arguments = '';
  public success = false;
  public message = '';
  public durationMs = 0;
}

export class McpCallLogListInModel {
  /** giorni all'indietro da oggi */
  public days = 30;
}

export class McpCallLogListOutModel extends ModelResult {
  public mcpCallLog: McpCallLogModel[] = [];
  /** vero = chiamate di tutta l'azienda (amministratori), falso = solo le proprie */
  public wholeCompany = false;
  /** vero se le righe sono piu' del massimo restituito */
  public truncated = false;
}
