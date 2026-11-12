import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpListInModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CmpService } from '../../../../../ClientServer-Service/Compliance/cmp.service';
import { cmpApriSezione } from '../cmp-widget-nav';

/*
 Widget "Formazione obbligatoria" (ANALISI_COMPLIANCE_WIDGET.md §3): dalla situazione della formazione (Cmp_FormazioneStato:
 corsi richiesti dalle mansioni attive dei dipendenti attivi) conta i corsi mancanti, scaduti e in scadenza e i dipendenti
 non in regola. Apre la sezione "Situazione formazione" di Sicurezza sul lavoro. Solo amministratori.
*/
@Component({
  selector: 'app-cmp-training-widget',
  templateUrl: './cmp-training-widget.component.html',
  styleUrls: ['./cmp-training-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class CmpTrainingWidgetComponent {

  public caricamento = true;
  public errore = false;
  public requisiti = 0;
  public mancanti = 0;
  public scaduti = 0;
  public inScadenza = 0;
  public dipendenti = 0;
  public nonInRegola = 0;

  constructor(private cmpService: CmpService, private navCtrl: NavController) {
    this.cmpService.FormazioneStato(new GenericRequest<CmpListInModel>(CmpListInModel)).subscribe({
      next: res => {
        if (res.success && res.data) {
          // stato: 0 mancante, 1 valido, 2 in scadenza, 3 scaduto
          const items = res.data.items ?? [];
          this.requisiti = items.length;
          this.mancanti = items.filter(x => x['stato'] === 0).length;
          this.scaduti = items.filter(x => x['stato'] === 3).length;
          this.inScadenza = items.filter(x => x['stato'] === 2).length;
          this.dipendenti = new Set(items.map(x => x['idDip_Anagrafica'])).size;
          this.nonInRegola = new Set(items.filter(x => x['stato'] === 0 || x['stato'] === 3).map(x => x['idDip_Anagrafica'])).size;
        }
        else
          this.errore = true;
        this.caricamento = false;
      },
      error: () => { this.errore = true; this.caricamento = false; }
    });
  }

  public apri(): void {
    cmpApriSezione(this.navCtrl, '/cmp81', 'stato');
  }
}
