import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner, IonIcon } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpEmptyInModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CmpWidgetScadenzaAreaModel, CmpWidgetScadenzeGetOutModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-widget-model';
import { CmpWidgetService } from '../../../../../ClientServer-Service/Compliance/cmp-widget.service';
import { cmpApriSezione, CMP_WIDGET_FONTI } from '../cmp-widget-nav';

/*
 Widget "Scadenze Compliance" (ANALISI_COMPLIANCE_WIDGET.md §2): scadenze aperte di sicurezza sul lavoro e GDPR, scadute e
 in scadenza entro il preavviso, per tipo (corsi, visite, nomine, documenti, responsabili esterni, violazioni).
 Ogni riga apre la sezione della pagina Sicurezza sul lavoro o Registro GDPR. Solo amministratori (Cmp_WidgetScadenzeGet).
*/
@Component({
  selector: 'app-cmp-deadlines-widget',
  templateUrl: './cmp-deadlines-widget.component.html',
  styleUrls: ['./cmp-deadlines-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, IonIcon, TranslocoPipe]
})
export class CmpDeadlinesWidgetComponent {

  public caricamento = true;
  public dati: CmpWidgetScadenzeGetOutModel | null = null;
  /** tipi con almeno una scadenza da guardare */
  public aree: CmpWidgetScadenzaAreaModel[] = [];

  constructor(private cmpWidgetService: CmpWidgetService, private navCtrl: NavController) {
    this.cmpWidgetService.ScadenzeGet(new GenericRequest<CmpEmptyInModel>(CmpEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.aree = (this.dati?.aree ?? []).filter(a => a.scadute > 0 || a.inScadenza > 0);
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public icona(fonte: string): string {
    return CMP_WIDGET_FONTI[fonte]?.icona ?? 'calendar-outline';
  }

  public apri(fonte: string): void {
    const f = CMP_WIDGET_FONTI[fonte];
    if (f)
      cmpApriSezione(this.navCtrl, f.url, f.sezione);
    else
      this.sicurezza();
  }

  public sicurezza(): void {
    cmpApriSezione(this.navCtrl, '/cmp81');
  }
}
