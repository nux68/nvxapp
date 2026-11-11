import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { IonButton, IonIcon, IonNote, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { Observable, map, of } from 'rxjs';
import { ApplicationType } from '../../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { WidgetDefinition } from '../../../../../Utility/infrastructure/widget.model';
import { DashboardStato, WidgetService } from '../../../../../Utility/infrastructure/widget.service';

/*
 Configurazione della propria dashboard (ANALISI_INFRASTRUCTURE_WIDGET.md), nelle pagine dei ruoli User, CompanyAdmin e
 CompanyPowerAdmin: widget scelti (ordine, rimozione) e widget disponibili raggruppati per applicativo.
 Il salvataggio lo comanda la pagina (barra dei pulsanti) con salva(); le chiavi salvate che il ramo non conosce
 o che oggi non sono disponibili si conservano in coda.
*/
@Component({
  selector: 'app-widget-config',
  templateUrl: './widget-config.component.html',
  styleUrls: ['./widget-config.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonButton, IonIcon, IonNote, IonSpinner, TranslocoPipe]
})
export class WidgetConfigComponent implements OnInit {

  public caricamento = true;
  public configurabile = false;
  public scelti: WidgetDefinition[] = [];
  /** gruppi dei widget non ancora scelti, per applicativo (solo i gruppi con widget) */
  public gruppi: { application: ApplicationType | null, widgets: WidgetDefinition[] }[] = [];

  /** chiavi salvate ma non disponibili ora (altro ramo, applicativo disattivato, ruolo): si conservano */
  private conservate: string[] = [];

  constructor(private widgetService: WidgetService) { }

  ngOnInit() {
    this.carica();
  }

  public carica(): void {
    this.caricamento = true;
    this.widgetService.Carica().subscribe(stato => {
      this.applica(stato);
      this.caricamento = false;
    });
  }

  private applica(stato: DashboardStato): void {
    this.configurabile = stato.configurabile;
    this.scelti = this.widgetService.Dashboard(stato);
    const disponibili = new Set(this.widgetService.Disponibili.map(w => w.key));
    this.conservate = stato.configurata ? stato.chiavi.filter(k => !disponibili.has(k)) : [];
    this.costruisciGruppi();
  }

  private costruisciGruppi(): void {
    const scelti = new Set(this.scelti.map(w => w.key));
    const gruppi: { application: ApplicationType | null, widgets: WidgetDefinition[] }[] = [];
    for (const w of this.widgetService.Disponibili.filter(x => !scelti.has(x.key))) {
      const app = w.application ?? null;
      let g = gruppi.find(x => x.application === app);
      if (!g) {
        g = { application: app, widgets: [] };
        gruppi.push(g);
      }
      g.widgets.push(w);
    }
    this.gruppi = gruppi;
  }

  /** chiave di traduzione del nome dell'applicativo */
  public nomeApplicativo(application: ApplicationType | null | undefined): string {
    return 'widgets.application.' + (application ?? 0);
  }

  public aggiungi(w: WidgetDefinition): void {
    this.scelti = [...this.scelti, w];
    this.costruisciGruppi();
  }

  public rimuovi(w: WidgetDefinition): void {
    this.scelti = this.scelti.filter(x => x.key !== w.key);
    this.costruisciGruppi();
  }

  public sposta(indice: number, verso: -1 | 1): void {
    const j = indice + verso;
    if (j < 0 || j >= this.scelti.length)
      return;
    const lista = [...this.scelti];
    [lista[indice], lista[j]] = [lista[j], lista[indice]];
    this.scelti = lista;
  }

  public ripristina(): void {
    this.scelti = this.widgetService.Predefiniti;
    this.costruisciGruppi();
  }

  /** salva la configurazione; true se salvata */
  public salva(): Observable<boolean> {
    if (!this.configurabile)
      return of(false);
    return this.widgetService.Salva([...this.scelti.map(w => w.key), ...this.conservate]).pipe(
      map(stato => {
        if (stato)
          this.applica(stato);
        return stato != null;
      }));
  }
}
