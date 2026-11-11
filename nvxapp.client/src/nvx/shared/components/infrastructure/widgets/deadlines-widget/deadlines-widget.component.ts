import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DeadlineModel, DeadlineSummaryInModel, DeadlineSummaryOutModel } from '../../../../../ClientServer-Service/Infrastructure/Deadline/Models/deadline-model';
import { DeadlineService } from '../../../../../ClientServer-Service/Infrastructure/Deadline/deadline.service';
import { LanguageService } from '../../../../../Utility/infrastructure/language.service';
import { NotificationNotifierService } from '../../../../../Utility/infrastructure/notification-notifier.service';

/*
 Widget "In scadenza" (Infrastructure, ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.3): scadenze scadute e in scadenza
 nei prossimi 30 giorni e le prime 5 (DeadlineSummary; gli amministratori vedono quelle dell'azienda, gli altri le proprie).
*/
@Component({
  selector: 'app-deadlines-widget',
  templateUrl: './deadlines-widget.component.html',
  styleUrls: ['./deadlines-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class DeadlinesWidgetComponent {

  public caricamento = true;
  public summary: DeadlineSummaryOutModel | null = null;

  constructor(private deadlineService: DeadlineService,
              private notificationNotifierService: NotificationNotifierService,
              private languageService: LanguageService,
              private navCtrl: NavController) {
    const request = new GenericRequest<DeadlineSummaryInModel>(DeadlineSummaryInModel);
    request.data.giorni = 30;
    request.data.massimo = 5;
    this.deadlineService.DeadlineSummary(request).subscribe({
      next: res => {
        this.summary = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public data(d: DeadlineModel): string {
    return d.dataScadenza ? formatDate(d.dataScadenza, 'shortDate', this.languageService.locale) : '';
  }

  // apre l'oggetto della scadenza, se c'e', altrimenti lo scadenzario
  public apri(d: DeadlineModel): void {
    if (d.url)
      this.notificationNotifierService.openUrl(d.url, d.urlState);
    else
      this.scadenzario();
  }

  public scadenzario(): void {
    this.navCtrl.navigateForward('/deadlines');
  }
}
