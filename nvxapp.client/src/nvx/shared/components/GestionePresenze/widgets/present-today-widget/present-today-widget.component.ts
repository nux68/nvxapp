import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DipWidgetService } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Widget/dip-widget.service';
import { Widget_PresenzeOggiGetInModel, Widget_PresenzeOggiGetOutModel } from '../../../../../ClientServer-Service/GestionePresenze/Dip_Widget/Models/dip-widget-model';

/*
 Widget "Presenti oggi" (Presenze, ANALISI_ATTENDANCETRACKING_WIDGET.md): dipendenti presenti e assenti oggi, con gli assenti
 giustificati; il CompanyAdmin vede solo i reparti che amministra (lo decide il server).
*/
@Component({
  selector: 'app-present-today-widget',
  templateUrl: './present-today-widget.component.html',
  styleUrls: ['./present-today-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class PresentTodayWidgetComponent {

  public caricamento = true;
  public dati: Widget_PresenzeOggiGetOutModel | null = null;

  constructor(private dipWidgetService: DipWidgetService, private navCtrl: NavController) {
    this.dipWidgetService.PresenzeOggiGet(new GenericRequest<Widget_PresenzeOggiGetInModel>(Widget_PresenzeOggiGetInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  /** percentuale dei presenti per la barra */
  public get percentuale(): number {
    return this.dati && this.dati.totale > 0 ? Math.round(this.dati.presenti * 100 / this.dati.totale) : 0;
  }

  public dettagli(): void {
    this.navCtrl.navigateForward('/presentstaff');
  }
}
