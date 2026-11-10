import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { DashboardConfigPageBase } from '../../../shared/components/infrastructure/widgets/widget-config/dashboard-config-page.base';

/*
 Pagina del ruolo CompanyPowerAdmin: l'utente configura i widget della propria dashboard (ANALISI_WIDGET.md).
*/
@Component({
  selector: 'app-company-power-admin-page',
  templateUrl: './company-power-admin-page.component.html',
  styleUrls: ['./company-power-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyPowerAdminPageComponent extends DashboardConfigPageBase {

  /** chiave di traduzione del titolo (tradotto nel template) */
  public readonly title = 'menu.companyPowerAdmin';

  constructor(navCtrl: NavController,
              userInterfaceService: UserInterfaceService) {
    super(navCtrl, userInterfaceService);
  }
}
