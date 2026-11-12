import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpEmptyInModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CmpWidgetGdprGetOutModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-widget-model';
import { CmpWidgetService } from '../../../../../ClientServer-Service/Compliance/cmp-widget.service';
import { cmpApriSezione } from '../cmp-widget-nav';

/*
 Widget "Registro GDPR" (ANALISI_COMPLIANCE_WIDGET.md §4): violazioni da notificare al Garante (in rosso se oltre le 72 ore),
 contratti dei responsabili esterni scaduti o in scadenza, trattamenti attivi. Solo amministratori (Cmp_WidgetGdprGet).
*/
@Component({
  selector: 'app-cmp-gdpr-widget',
  templateUrl: './cmp-gdpr-widget.component.html',
  styleUrls: ['./cmp-gdpr-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class CmpGdprWidgetComponent {

  public caricamento = true;
  public dati: CmpWidgetGdprGetOutModel | null = null;

  constructor(private cmpWidgetService: CmpWidgetService, private navCtrl: NavController) {
    this.cmpWidgetService.GdprGet(new GenericRequest<CmpEmptyInModel>(CmpEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public apri(sezione?: string): void {
    cmpApriSezione(this.navCtrl, '/cmpgdpr', sezione);
  }
}
