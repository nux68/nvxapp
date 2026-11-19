import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { NavController, IonSpinner, IonIcon } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { ApplicationType } from '../../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpEmptyInModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CmpWidgetScadenzaAreaModel, CmpWidgetScadenzeGetOutModel } from '../../../../../ClientServer-Service/Compliance/Models/cmp-widget-model';
import { CmpWidgetService } from '../../../../../ClientServer-Service/Compliance/cmp-widget.service';
import { DataChangedNotifierService } from '../../../../../Utility/infrastructure/data-changed-notifier.service';
import { cmpApriSezione, CMP_WIDGET_FONTI } from '../cmp-widget-nav';

/*
 Widget "Scadenze Compliance" (ANALISI_COMPLIANCE_WIDGET.md §2): scadenze aperte di sicurezza sul lavoro e GDPR, scadute e
 in scadenza entro il preavviso, per tipo (corsi, visite, nomine, documenti, responsabili esterni, violazioni).
 Ogni riga apre la sezione della pagina Sicurezza sul lavoro o Registro GDPR. Solo amministratori (Cmp_WidgetScadenzeGet).
 I dati si rileggono all'avviso DataChangedNotifier di Compliance (comando di un assistente AI via MCP,
 ANALISI_INFRASTRUCTURE_MCP.md MCP-43): il widget non ha ionViewWillEnter, la sottoscrizione vive con il componente.
*/
@Component({
  selector: 'app-cmp-deadlines-widget',
  templateUrl: './cmp-deadlines-widget.component.html',
  styleUrls: ['./cmp-deadlines-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, IonIcon, TranslocoPipe]
})
export class CmpDeadlinesWidgetComponent implements OnDestroy {

  public caricamento = true;
  public dati: CmpWidgetScadenzeGetOutModel | null = null;
  /** tipi con almeno una scadenza da guardare */
  public aree: CmpWidgetScadenzaAreaModel[] = [];
  private readonly dataChangedSub: Subscription;

  constructor(private cmpWidgetService: CmpWidgetService, private navCtrl: NavController, dataChangedNotifierService: DataChangedNotifierService) {
    this.carica();
    this.dataChangedSub = dataChangedNotifierService.of(ApplicationType.Compliance).subscribe(() => this.carica());
  }

  ngOnDestroy(): void {
    this.dataChangedSub.unsubscribe();
  }

  /** lettura dei dati (alla creazione e a ogni avviso di modifica) */
  private carica(): void {
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
