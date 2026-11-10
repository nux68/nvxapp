import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { DashboardConfigPageBase } from '../../../shared/components/infrastructure/widgets/widget-config/dashboard-config-page.base';

/*
 Pagina del ruolo User: l'utente configura i widget della propria dashboard (ANALISI_WIDGET.md).
*/
@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class UserPageComponent extends DashboardConfigPageBase {

  /** chiave di traduzione del titolo (tradotto nel template) */
  public readonly title = 'menu.preferences';

  constructor(navCtrl: NavController,
              userInterfaceService: UserInterfaceService) {
    super(navCtrl, userInterfaceService);
  }
}
