import { Component, ChangeDetectionStrategy, computed, signal } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DashboardStato, WidgetService } from '../../../Utility/infrastructure/widget.service';
import { WidgetDefinition } from '../../../Utility/infrastructure/widget.model';
import { disponiWidget, WidgetRiga } from '../../../Utility/infrastructure/widget-layout';

/*
 Dashboard (pagina iniziale dopo il login): saluto, collegamenti alle voci di menu e widget scelti dall'utente
 (ANALISI_INFRASTRUCTURE_WIDGET.md).
 Chi non ha ancora configurato la dashboard vede i widget predefiniti; i widget si configurano nella pagina
 del proprio ruolo (User, CompanyAdmin, CompanyPowerAdmin).
*/
@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DashboardPageComponent {

  public title!: string;

  public widgets: WidgetDefinition[] = [];
  /** widget disposti in righe secondo numero e dimensioni (widget-layout.ts) */
  public righe: WidgetRiga[] = [];
  public configurabile = false;
  /** stato salvato: i collegamenti si ricalcolano anche quando il menu si compone dopo (signal delle voci) */
  private readonly stato = signal<DashboardStato | null>(null);
  public readonly collegamenti = computed(() => {
    const stato = this.stato();
    return stato ? this.widgetService.Collegamenti(stato) : [];
  });
  public caricamento = true;
  /** richiesta in corso: con ingressi ravvicinati vale solo l'ultima risposta */
  private richiesta = 0;

  constructor(public userNavigationService: UserNavigationService,
              private widgetService: WidgetService,
              public navCtrl: NavController,
              private transloco: TranslocoService
  ) {
    this.title = this.transloco.translate('pages.dashboard.title');
  }

  // a ogni ingresso l'elenco si svuota e si ricostruisce: i widget si ricreano (ricaricando i dati) sempre
  // nell'ordine giusto, senza riconciliare righe vecchie e nuove
  ionViewWillEnter() {
    const numero = ++this.richiesta;
    this.caricamento = true;
    this.righe = [];
    this.widgetService.Carica().subscribe(stato => {
      if (numero !== this.richiesta)
        return;
      this.configurabile = stato.configurabile;
      this.stato.set(stato);
      this.widgets = this.widgetService.Dashboard(stato);
      this.righe = disponiWidget(this.widgets);
      this.caricamento = false;
    });
  }

  // nome del saluto per l'utente corrente (ultimo della cronologia di navigazione/impersonificazione):
  // il nome dell'anagrafica se c'e', altrimenti lo username
  public get userName(): string | null {
    const cronology = this.userNavigationService.userCronology;
    const userData = cronology && cronology.length > 0 ? cronology[cronology.length - 1].userData : null;
    if (!userData)
      return null;
    return userData.nome?.trim() || userData.userName || null;
  }

  public get paginaConfigurazione(): string | null {
    return this.configurabile ? this.widgetService.PaginaConfigurazione : null;
  }

  public apri(url: string): void {
    this.navCtrl.navigateRoot(url);
  }

  public configura(): void {
    const pagina = this.paginaConfigurazione;
    if (pagina)
      this.navCtrl.navigateForward(pagina);
  }
}
