import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { RequestItemModel } from '../../ClientServer-Service/Infrastructure/Request/Models/request-model';
import { ButtonItem } from './user-interface.service';

/*
 Richieste da approvare comuni (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.3).
 Le pagine /requests e /requestsadmin sono di Infrastructure; ogni ramo che porta richieste puo' aggiungere, con un
 fornitore iRequestUiProvider (request-ui-<ramo>.service.ts, vuoto in Infrastructure):
   - azioni sulle righe con item.extra uguale al codice dell'azione (es. il dettaglio della nota spese)
   - le voci "Nuova richiesta" della pagina "Le mie richieste" (pagine di invio del modulo)
*/

/** applicativi che portano richieste da approvare: le voci di menu compaiono se l'azienda ne ha almeno uno */
export const APPLICATIVI_RICHIESTE: ApplicationType[] = [ApplicationType.AttendanceTracking, ApplicationType.Assets];

export interface RequestUiAction {
  /** codice di RequestItemModel.extra a cui si applica */
  extra: string;
  /** pulsante (UserInterfaceService.Btn_*), creato a ogni apertura della pagina */
  button: () => ButtonItem;
  /** esecuzione; admin = pagina "Richieste da approvare" */
  run: (item: RequestItemModel, admin: boolean) => void;
}

export interface RequestNewItem {
  /** chiave transloco del testo */
  title: string;
  url: string;
  /** icona nell'action sheet "Nuova richiesta" (default add-circle-outline) */
  icon?: string;
  /** ruoli che possono usare la pagina di invio (assente = tutti quelli della pagina) */
  roles?: string[];
  /** applicativo della pagina (lo imposta RequestUiService se assente) */
  application?: ApplicationType;
}

export interface iRequestUiProvider {
  readonly Actions: RequestUiAction[];
  readonly NewRequests: RequestNewItem[];
}
