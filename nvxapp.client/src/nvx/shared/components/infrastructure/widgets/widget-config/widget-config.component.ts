import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { IonButton, IonIcon, IonLabel, IonNote, IonSegment, IonSegmentButton, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { Observable, map, of } from 'rxjs';
import { ApplicationType } from '../../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { WidgetDefinition } from '../../../../../Utility/infrastructure/widget.model';
import { DashboardStato, WidgetService } from '../../../../../Utility/infrastructure/widget.service';
import { VoceNavigabile } from '../../../../../Utility/infrastructure/main-menu.service';

/*
 Configurazione della propria dashboard (ANALISI_INFRASTRUCTURE_WIDGET.md), nelle pagine dei ruoli User, CompanyAdmin e
 CompanyPowerAdmin, in due schede: i widget (scelti, con ordine e rimozione, e disponibili raggruppati per applicativo)
 e i collegamenti alle voci di menu (scelti e disponibili raggruppati per sezione del menu).
 Il salvataggio lo comanda la pagina (barra dei pulsanti) con salva(); le chiavi salvate che il ramo non conosce
 o che oggi non sono disponibili si conservano in coda.
*/
@Component({
  selector: 'app-widget-config',
  templateUrl: './widget-config.component.html',
  styleUrls: ['./widget-config.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonButton, IonIcon, IonLabel, IonNote, IonSegment, IonSegmentButton, IonSpinner, TranslocoPipe]
})
export class WidgetConfigComponent implements OnInit {

  public caricamento = true;
  public configurabile = false;
  public scelti: WidgetDefinition[] = [];
  /** gruppi dei widget non ancora scelti, per applicativo (solo i gruppi con widget) */
  public gruppi: { application: ApplicationType | null, widgets: WidgetDefinition[] }[] = [];

  /** scheda aperta: widget o voci di menu */
  public scheda: 'widget' | 'menu' = 'widget';
  /** voci di menu collegate alla dashboard, nell'ordine */
  public menuScelti: (VoceNavigabile & { key: string })[] = [];
  /** voci di menu non ancora collegate, per sezione del menu */
  public menuGruppi: { section: string, voci: (VoceNavigabile & { key: string })[] }[] = [];

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
    this.menuScelti = this.widgetService.Collegamenti(stato);
    const disponibili = new Set([...this.widgetService.Disponibili.map(w => w.key), ...this.widgetService.VociMenuDisponibili.map(v => v.key)]);
    this.conservate = stato.configurata ? stato.chiavi.filter(k => !disponibili.has(k)) : [];
    this.costruisciGruppi();
    this.costruisciGruppiMenu();
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

  private costruisciGruppiMenu(): void {
    const scelti = new Set(this.menuScelti.map(v => v.key));
    const gruppi: { section: string, voci: (VoceNavigabile & { key: string })[] }[] = [];
    for (const v of this.widgetService.VociMenuDisponibili.filter(x => !scelti.has(x.key))) {
      let g = gruppi.find(x => x.section === v.section);
      if (!g) {
        g = { section: v.section, voci: [] };
        gruppi.push(g);
      }
      g.voci.push(v);
    }
    this.menuGruppi = gruppi;
  }

  public aggiungiMenu(v: VoceNavigabile & { key: string }): void {
    this.menuScelti = [...this.menuScelti, v];
    this.costruisciGruppiMenu();
  }

  public rimuoviMenu(v: VoceNavigabile & { key: string }): void {
    this.menuScelti = this.menuScelti.filter(x => x.key !== v.key);
    this.costruisciGruppiMenu();
  }

  public spostaMenu(indice: number, verso: -1 | 1): void {
    const j = indice + verso;
    if (j < 0 || j >= this.menuScelti.length)
      return;
    const lista = [...this.menuScelti];
    [lista[indice], lista[j]] = [lista[j], lista[indice]];
    this.menuScelti = lista;
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
    return this.widgetService.Salva([...this.scelti.map(w => w.key), ...this.menuScelti.map(v => v.key), ...this.conservate]).pipe(
      map(stato => {
        if (stato)
          this.applica(stato);
        return stato != null;
      }));
  }
}
