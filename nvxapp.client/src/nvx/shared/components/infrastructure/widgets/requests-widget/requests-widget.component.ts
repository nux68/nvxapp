import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '../../../../../Utility/infrastructure/auth.service';
import { RequestNotifierService } from '../../../../../Utility/infrastructure/request-notifier.service';

/*
 Widget "Richieste" comune a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.4): contatori del
 RequestNotifierService (gli stessi dei badge del menu), aggiornati dall'evento SignalR "RequestNotifier".
   - amministratori (CompanyPowerAdmin, CompanyAdmin): richieste da approvare, e le proprie in attesa se ne hanno
   - utenti: proprie richieste in attesa
*/
@Component({
  selector: 'app-infrastructure-requests-widget',
  templateUrl: './requests-widget.component.html',
  styleUrls: ['./requests-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [TranslocoPipe]
})
export class RequestsWidgetComponent {

  public readonly admin: boolean;

  constructor(public requestNotifierService: RequestNotifierService,
              authService: AuthService,
              private navCtrl: NavController) {
    this.admin = authService.IsInGroupCompanyAdmin;
    this.requestNotifierService.refreshCount();
  }

  public apri(admin: boolean): void {
    this.navCtrl.navigateForward(admin ? '/requestsadmin' : '/requests');
  }
}
