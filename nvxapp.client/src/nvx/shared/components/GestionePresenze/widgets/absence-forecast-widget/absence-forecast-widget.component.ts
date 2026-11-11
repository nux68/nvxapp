import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { DatePipe, DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipAnomalieService } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/dip-anomalie.service';
import { PrevisioneGiornoModel, Widget_PrevisioneAssenzeGetInModel, Widget_PrevisioneAssenzeGetOutModel } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Anomalie/Models/dip-anomalie-model';

/*
 Widget "Assenze previste" (Presenze, ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md §5, AT-AI.3): prossimi 7 giorni, assenze
 previste (statistica dello storico e piano ferie), giorni di picco. Barre per i giorni lavorativi.
*/
@Component({
  selector: 'app-absence-forecast-widget',
  templateUrl: './absence-forecast-widget.component.html',
  styleUrls: ['./absence-forecast-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe, DatePipe, DecimalPipe]
})
export class AbsenceForecastWidgetComponent {

  public caricamento = true;
  public dati: Widget_PrevisioneAssenzeGetOutModel | null = null;

  constructor(private dipAnomalieService: DipAnomalieService, private navCtrl: NavController) {
    this.dipAnomalieService.PrevisioneAssenzeWidget(new GenericRequest<Widget_PrevisioneAssenzeGetInModel>(Widget_PrevisioneAssenzeGetInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public get lavorativi(): PrevisioneGiornoModel[] {
    return (this.dati?.giorni ?? []).filter(g => g.inServizio > 0);
  }

  public get picchi(): number {
    return this.lavorativi.filter(g => g.picco).length;
  }

  /** percentuale delle assenze previste sui dipendenti attesi */
  public perc(g: PrevisioneGiornoModel): number {
    return g.inServizio > 0 ? Math.min(100, Math.round(g.previste * 100 / g.inServizio)) : 0;
  }

  public apri(): void {
    this.navCtrl.navigateForward('/attendanceanomalies');
  }
}
