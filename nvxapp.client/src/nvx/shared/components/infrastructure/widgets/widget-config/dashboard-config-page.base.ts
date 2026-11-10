import { Directive, ViewChild } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../../../Utility/infrastructure/user-interface.service';
import { WidgetConfigComponent } from './widget-config.component';

/*
 Base delle pagine dei ruoli (User, CompanyAdmin, CompanyPowerAdmin) in cui l'utente configura la propria dashboard
 (ANALISI_WIDGET.md): Conferma salva e apre la dashboard, Annulla ricarica la configurazione salvata.
*/
@Directive()
export abstract class DashboardConfigPageBase {

  @ViewChild(WidgetConfigComponent) widgetConfig?: WidgetConfigComponent;

  public buttonbar: ButtonItem[] = [];

  constructor(protected navCtrl: NavController,
              userInterfaceService: UserInterfaceService) {
    const btnConferma = userInterfaceService.Btn_Conferma;
    btnConferma.event = () => this.salva();
    const btnAnnulla = userInterfaceService.Btn_Annulla;
    btnAnnulla.event = () => this.widgetConfig?.carica();
    this.buttonbar = [btnConferma, btnAnnulla];
  }

  ionViewWillEnter() {
    // rientrando nella pagina si riparte dalla configurazione salvata (gli applicativi attivi possono essere cambiati)
    this.widgetConfig?.carica();
  }

  private salva(): void {
    this.widgetConfig?.salva().subscribe(ok => {
      if (ok)
        this.navCtrl.navigateRoot('/dashboard');
    });
  }
}
