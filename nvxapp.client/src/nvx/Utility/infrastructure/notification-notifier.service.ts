import { Injectable, Signal, signal } from '@angular/core';
import { NavController } from '@ionic/angular';
import { Observable, Subject } from 'rxjs';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { NotificationCountInModel, NotificationNewData } from '../../ClientServer-Service/Infrastructure/Notification/Models/notification-model';
import { NotificationService } from '../../ClientServer-Service/Infrastructure/Notification/notification.service';
import { AcknowledgementListInModel } from '../../ClientServer-Service/Infrastructure/Acknowledgement/Models/acknowledgement-model';
import { AcknowledgementService } from '../../ClientServer-Service/Infrastructure/Acknowledgement/acknowledgement.service';
import { AuthService } from './auth.service';

// ruoli di chi lavora in un'azienda: scadenzario e prese visione (stessi ruoli delle voci di menu e di RoleGuard4CompanyUser)
export const RUOLI_AZIENDA = ['CompanyPowerAdmin', 'CompanyAdmin', 'User'];

/*
 Contatori delle notifiche (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4 e INF-SVC.6), mostrati come badge del menu:
   - nonLette: notifiche non lette dell'utente (voce "Notifiche")
   - daConfermare: prese visione da confermare (voce "Prese visione", solo per gli utenti di un'azienda)
 Si ricalcolano al login (app.component), all'evento SignalR "NotificationNew" (anche le richieste di presa visione arrivano
 come notifica) e dopo letture, cancellazioni e conferme fatte nelle pagine.
 changed$: le pagine aperte (notifiche, prese visione) si ricaricano all'arrivo di una notifica.
 L'avviso a comparsa, se l'utente lo vuole, arriva a parte con l'evento "UserNotifier".
*/
@Injectable({
  providedIn: 'root'
})
export class NotificationNotifierService {

  private readonly _nonLette = signal(0);
  public readonly nonLette: Signal<number> = this._nonLette.asReadonly();

  private readonly _daConfermare = signal(0);
  public readonly daConfermare: Signal<number> = this._daConfermare.asReadonly();

  private readonly _changed = new Subject<NotificationNewData>();
  public readonly changed$: Observable<NotificationNewData> = this._changed.asObservable();

  constructor(private notificationService: NotificationService,
              private acknowledgementService: AcknowledgementService,
              private authService: AuthService,
              private navCtrl: NavController) { }

  // nuova notifica arrivata dal server
  public notificationNew(data: NotificationNewData): void {
    this._changed.next(data);
    this.refreshCount();
  }

  // ricalcola i contatori (dopo il login, un'impersonificazione, all'arrivo di una notifica)
  public refreshCount(): void {
    if (!this.authService.Token) {
      this._nonLette.set(0);
      this._daConfermare.set(0);
      return;
    }

    this.notificationService.NotificationCount(new GenericRequest<NotificationCountInModel>(NotificationCountInModel)).subscribe(res => {
      this._nonLette.set(res.success && res.data ? res.data.nonLette : 0);
    });
    this.refreshAcknowledgementCount();
  }

  // prese visione da confermare: solo per chi lavora in un'azienda
  public refreshAcknowledgementCount(): void {
    if (!this.authService.Token || !this.authService.RoleNames.some(r => RUOLI_AZIENDA.includes(r))) {
      this._daConfermare.set(0);
      return;
    }

    const request = new GenericRequest<AcknowledgementListInModel>(AcknowledgementListInModel);
    request.data.daConfermare = true;
    this.acknowledgementService.AcknowledgementList(request).subscribe(res => {
      this._daConfermare.set(res.success && res.data ? res.data.daConfermare : 0);
    });
  }

  // valori restituiti dalle chiamate delle pagine (lettura, cancellazione, conferma): evitano una nuova richiesta
  public setNonLette(value: number): void {
    this._nonLette.set(value);
  }

  public setDaConfermare(value: number): void {
    this._daConfermare.set(value);
  }

  /*
   Apre la pagina dell'oggetto di una notifica, scadenza o presa visione: url del client e stato in JSON (stringa)
   passato alla pagina come fa UserNotifierService.
  */
  public openUrl(url: string | null, urlState: string | null): void {
    if (!url)
      return;

    let state: any = {};
    if (urlState) {
      try {
        state = JSON.parse(urlState) ?? {};
      }
      catch {
        state = {};
      }
    }
    this.navCtrl.navigateForward(url, { state });
  }
}
