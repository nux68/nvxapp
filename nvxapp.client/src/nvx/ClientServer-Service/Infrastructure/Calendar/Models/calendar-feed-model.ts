import { ModelResult } from "../../../ModelsBase/model-result";


// feed iCalendar personale dell'utente nell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.3)
export class CalendarFeedModel {
  /** indirizzo https del feed (Google Calendar, Outlook) */
  public url = '';
  /** stesso indirizzo con lo schema webcal:// (apre direttamente Apple Calendar / Outlook) */
  public webcalUrl = '';
  public dataCreazione: string | null = null;
  public dataUltimoAccesso: string | null = null;
}


export class CalendarFeedGetInModel {
  /** crea il feed se manca (false = restituisce solo quello esistente) */
  public crea = true;
}

export class CalendarFeedGetOutModel extends ModelResult {
  /** null = nessun feed */
  public feed: CalendarFeedModel | null = null;
}


export class CalendarFeedRegenerateInModel {
}

export class CalendarFeedRegenerateOutModel extends ModelResult {
  public feed: CalendarFeedModel | null = null;
}


export class CalendarFeedDeleteInModel {
}

export class CalendarFeedDeleteOutModel extends ModelResult {
}
