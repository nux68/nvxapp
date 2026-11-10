import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { DashboardConfigPageBase } from '../../../shared/components/infrastructure/widgets/widget-config/dashboard-config-page.base';

/*
 Pagina del ruolo CompanyAdmin: l'utente configura i widget della propria dashboard (ANALISI_WIDGET.md).
*/
@Component({
  selector: 'app-company-admin-page',
  templateUrl: './company-admin-page.component.html',
  styleUrls: ['./company-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyAdminPageComponent extends DashboardConfigPageBase {

  /** chiave di traduzione del titolo (tradotto nel template) */
  public readonly title = 'menu.companyAdmin';

  constructor(navCtrl: NavController,
              userInterfaceService: UserInterfaceService) {
    super(navCtrl, userInterfaceService);
  }
}
