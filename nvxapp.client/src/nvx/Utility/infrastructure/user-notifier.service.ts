import { Injectable } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';

/*
 Avvisi a comparsa inviati dal server agli utenti collegati (evento SignalR "UserNotifier", collegato in
 app.component), usabili da ogni applicativo (es. promemoria del CRM): titolo e messaggio arrivano gia' tradotti;
 con un indirizzo compare il pulsante per aprire la pagina.
*/
@Injectable({
  providedIn: 'root'
})
export class UserNotifierService {

  constructor(private toastCtrl: ToastController,
              private navCtrl: NavController,
              private transloco: TranslocoService) { }

  public userNotifier(data: UserNotificationData): void {
    this.toastCtrl.create({
      header: data.title,
      message: data.message,
      duration: 10000,
      position: 'top',
      buttons: data.url ? [{
        text: this.transloco.translate('common.buttons.open'),
        handler: () => { this.navCtrl.navigateForward(data.url!, { state: data.state ?? {} }); }
      }] : [{ text: 'OK', role: 'cancel' }]
    }).then(t => t.present());
  }
}

export class UserNotificationData {
  public title = '';
  public message = '';
  public url: string | null = null;
  public state: any = null;
}
