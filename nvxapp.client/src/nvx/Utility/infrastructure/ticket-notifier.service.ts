import { Injectable, Signal, signal } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, Subject } from 'rxjs';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { TicketCountInModel, TicketNotifierData } from '../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { TicketService } from '../../ClientServer-Service/Infrastructure/SupportTicket/ticket.service';
import { AuthService } from './auth.service';

/*
 Notifiche dei ticket di assistenza (evento SignalR "TicketNotifier", collegato in app.component):
   - contatore dei ticket "in carico a me" mostrato sulla voce di menu
   - avviso a comparsa con il pulsante per aprire il ticket
   - changed$: le pagine dei ticket aperte si ricaricano
*/
@Injectable({
  providedIn: 'root'
})
export class TicketNotifierService {

  private readonly _inCarico = signal(0);
  public readonly inCarico: Signal<number> = this._inCarico.asReadonly();

  private readonly _changed = new Subject<TicketNotifierData>();
  public readonly changed$: Observable<TicketNotifierData> = this._changed.asObservable();

  constructor(private ticketService: TicketService,
              private authService: AuthService,
              private toastCtrl: ToastController,
              private navCtrl: NavController,
              private transloco: TranslocoService) { }

  // notifica arrivata dal server
  public ticketNotifier(data: TicketNotifierData): void {
    this._changed.next(data);
    this.refreshCount();

    this.toastCtrl.create({
      message: this.transloco.translate('ticket.notification.message', {
        id: data.idTicket,
        oggetto: data.oggetto,
        action: this.transloco.translate('ticket.action.' + data.action)
      }),
      duration: 6000,
      position: 'top',
      buttons: [{
        text: this.transloco.translate('common.buttons.open'),
        handler: () => { this.navCtrl.navigateForward('/ticketdetail', { state: { id: data.idTicket } }); }
      }]
    }).then(t => t.present());
  }

  // ricalcola il contatore (dopo il login, dopo un passaggio, all'arrivo di una notifica)
  public refreshCount(): void {
    if (!this.authService.Token) {
      this._inCarico.set(0);
      return;
    }

    const request = new GenericRequest<TicketCountInModel>(TicketCountInModel);
    this.ticketService.TicketCount(request).subscribe(res => {
      this._inCarico.set(res.success && res.data ? res.data.inCarico : 0);
    });
  }
}
