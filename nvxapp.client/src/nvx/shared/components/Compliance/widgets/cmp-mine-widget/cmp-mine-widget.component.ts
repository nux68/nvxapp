import { Component, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpListInModel, CmpMineOutModel, CmpRiga } from '../../../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CmpService } from '../../../../../ClientServer-Service/Compliance/cmp.service';
import { cmpApriSezione } from '../cmp-widget-nav';

/*
 Widget "I miei adempimenti" (ANALISI_COMPLIANCE_WIDGET.md §5), per ogni utente dell'azienda collegato a un dipendente
 (Cmp_Mine): corsi scaduti o in scadenza, DPI ricevuti da confermare, data della prossima visita medica (solo la data:
 esito e dati sanitari non compaiono in dashboard), nomine in corso. Apre "I miei adempimenti".
*/
@Component({
  selector: 'app-cmp-mine-widget',
  templateUrl: './cmp-mine-widget.component.html',
  styleUrls: ['./cmp-mine-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe, DatePipe]
})
export class CmpMineWidgetComponent {

  public caricamento = true;
  public dati: CmpMineOutModel | null = null;
  public corsiScaduti = 0;
  public corsiInScadenza = 0;
  public dpiDaConfermare = 0;
  public nomineInCorso = 0;
  /** visita piu' recente (quella con la scadenza valida), null se nessuna */
  public visita: CmpRiga | null = null;

  constructor(private cmpService: CmpService, private navCtrl: NavController) {
    this.cmpService.Mine(new GenericRequest<CmpListInModel>(CmpListInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        if (this.dati) {
          const oggi = new Date();
          oggi.setHours(0, 0, 0, 0);
          this.corsiScaduti = this.dati.formazione.filter(x => x.scaduta).length;
          this.corsiInScadenza = this.dati.formazione.filter(x => x.inScadenza).length;
          this.dpiDaConfermare = this.dati.consegne.filter(x => x['presaVisioneRichiesta'] && !x['dataPresaVisione']).length;
          this.nomineInCorso = this.dati.nomine.filter(x => !x.scaduta && (!x['dataInizio'] || new Date(x['dataInizio']) <= oggi)).length;
          this.visita = this.dati.visite.find(x => !x.rinnovata && x['dataScadenza']) ?? null;
        }
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public apri(): void {
    cmpApriSezione(this.navCtrl, '/cmpmine');
  }
}
