import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipWidgetService } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Widget/dip-widget.service';
import { Widget_RichiesteGetInModel, Widget_RichiesteGetOutModel } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Widget/Models/dip-widget-model';

/*
 Widget "Richieste" (Presenze, ANALISI_ATTENDANCETRACKING_WIDGET.md):
   - amministratori (CompanyPowerAdmin, CompanyAdmin): richieste da approvare e approvate nel mese;
   - utenti: proprie richieste in attesa e approvate nel mese.
*/
@Component({
  selector: 'app-requests-widget',
  templateUrl: './requests-widget.component.html',
  styleUrls: ['./requests-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class RequestsWidgetComponent {

  public caricamento = true;
  public dati: Widget_RichiesteGetOutModel | null = null;

  constructor(private dipWidgetService: DipWidgetService, private navCtrl: NavController) {
    this.dipWidgetService.RichiesteGet(new GenericRequest<Widget_RichiesteGetInModel>(Widget_RichiesteGetInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  /** elenco delle richieste: dell'azienda per gli amministratori, proprie per gli utenti */
  public apri(): void {
    this.navCtrl.navigateForward(this.dati?.admin ? '/requestlistadmin' : '/requestlistuser');
  }
}
