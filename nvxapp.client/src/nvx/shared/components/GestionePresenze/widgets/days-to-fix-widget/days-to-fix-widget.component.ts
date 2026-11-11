import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { DatePipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipAnomalieService } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/dip-anomalie.service';
import { Widget_GiornateDaSistemareGetInModel, Widget_GiornateDaSistemareGetOutModel } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/Models/dip-anomalie-model';

/*
 Widget "Giornate da sistemare" (Presenze, ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md, AT-AI.4): giornate da sistemare prima
 dell'export paghe e da verificare (nei primi 10 giorni del mese anche il mese precedente), fino a ieri.
*/
@Component({
  selector: 'app-days-to-fix-widget',
  templateUrl: './days-to-fix-widget.component.html',
  styleUrls: ['./days-to-fix-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe, DatePipe]
})
export class DaysToFixWidgetComponent {

  public caricamento = true;
  public dati: Widget_GiornateDaSistemareGetOutModel | null = null;

  constructor(private dipAnomalieService: DipAnomalieService, private navCtrl: NavController) {
    this.dipAnomalieService.GiornateDaSistemareWidget(new GenericRequest<Widget_GiornateDaSistemareGetInModel>(Widget_GiornateDaSistemareGetInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/attendanceanomalies');
  }
}
