import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { WidgetService } from '../../../Utility/infrastructure/widget.service';
import { WidgetDefinition } from '../../../Utility/infrastructure/widget.model';

/*
 Dashboard (pagina iniziale dopo il login): saluto e widget scelti dall'utente (ANALISI_WIDGET.md).
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
  public configurabile = false;
  public caricamento = true;
  /** cambia a ogni ingresso: i widget si ricreano e ricaricano i loro dati */
  public giro = 0;

  constructor(public userNavigationService: UserNavigationService,
              private widgetService: WidgetService,
              public navCtrl: NavController,
              private transloco: TranslocoService
  ) {
    this.title = this.transloco.translate('pages.dashboard.title');
  }

  ionViewWillEnter() {
    this.widgetService.Carica().subscribe(stato => {
      this.configurabile = stato.configurabile;
      this.widgets = this.widgetService.Dashboard(stato);
      this.giro++;
      this.caricamento = false;
    });
  }

  // utente corrente (ultimo della cronologia di navigazione/impersonificazione)
  public get userName(): string | null {
    const cronology = this.userNavigationService.userCronology;
    return cronology && cronology.length > 0 ? cronology[cronology.length - 1].userData?.userName ?? null : null;
  }

  public get paginaConfigurazione(): string | null {
    return this.configurabile ? this.widgetService.PaginaConfigurazione : null;
  }

  public configura(): void {
    const pagina = this.paginaConfigurazione;
    if (pagina)
      this.navCtrl.navigateForward(pagina);
  }
}
