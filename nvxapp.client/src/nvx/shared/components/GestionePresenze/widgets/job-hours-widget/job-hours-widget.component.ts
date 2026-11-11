import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { DecimalPipe } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { AzCommessaCdgService } from '../../../../../ClientServer-Service/GestionePresenze/Az_CommessaCdg/az-commessa-cdg.service';
import { Widget_OreCommesseGetInModel, Widget_OreCommesseGetOutModel } from '../../../../../ClientServer-Service/GestionePresenze/Az_CommessaCdg/Models/az-commessa-cdg-model';

/*
 Widget "Ore sulle commesse" (Presenze, ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md, AT-CDG.3): ore del mese corrente
 per commessa (prime 5) e, se nella pagina "Marginalita' commesse" e' stato impostato un costo orario predefinito
 (ricordato dal browser), il costo stimato.
*/
@Component({
  selector: 'app-job-hours-widget',
  templateUrl: './job-hours-widget.component.html',
  styleUrls: ['./job-hours-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe, DecimalPipe]
})
export class JobHoursWidgetComponent {

  public caricamento = true;
  public dati: Widget_OreCommesseGetOutModel | null = null;

  constructor(private cdgService: AzCommessaCdgService, private navCtrl: NavController) {
    const request = new GenericRequest<Widget_OreCommesseGetInModel>(Widget_OreCommesseGetInModel);
    request.data.costoOrarioPredefinito = this.cdgService.ParametriSalvati()?.costoOrarioPredefinito ?? null;
    this.cdgService.OreCommesseWidget(request).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public ore(minuti: number): string {
    return `${Math.floor(minuti / 60)}:${String(minuti % 60).padStart(2, '0')}`;
  }

  public perc(minuti: number): number {
    const max = Math.max(...(this.dati?.commesse ?? []).map(c => c.minuti), 1);
    return Math.round(minuti * 100 / max);
  }

  public apri(): void {
    this.navCtrl.navigateForward('/jobmargins');
  }
}
