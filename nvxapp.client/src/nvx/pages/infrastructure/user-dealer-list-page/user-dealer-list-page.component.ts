import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';

import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserCompanyListModel, UserCompanyListInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { UserDealerListModel, UserDealerListInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-dealer-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-dealer-list-page',
  templateUrl: './user-dealer-list-page.component.html',
  styleUrls: ['./user-dealer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone:false
})
export class UserDealerListPageComponent {

  public title!: string;
  public searchText!: string;
  public userDealerList: UserDealerListModel[] | null = null;
  public readonly columns: NvxListColumn<UserDealerListModel>[] = [{ key: 'descrizione', title: this.transloco.translate('pages.userDealerList.columnUser') }];
  public readonly actions: NvxListAction<UserDealerListModel>[];
  public readonly isDisabled = (item: UserDealerListModel) => item.disabled;
  public readonly isHighlighted = (item: UserDealerListModel) => this.isPowerAdmin(item);
  public readonly iconOf = (item: UserDealerListModel) => this.isPowerAdmin(item) ? 'person-add-outline' : 'person-outline';
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.userDealerList.title');

    
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.actions = [{ button: this.btnEdit }];
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserDealerListInModel> = new GenericRequest<UserDealerListInModel>(UserDealerListInModel);
    this.accountService.UserDealerList(request).subscribe(res => {

      this.userDealerList = res.data.userDealerList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem(this.transloco.translate('pages.userDealerList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/userdealeredit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }



  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/userdealeredit', {
      state: { id: item.idUserDealer }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.DealerAdmin);

    const filteredUserDealerList = this.userDealerList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserDealerList;

  }

  getPowerAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.DealerPowerAdmin);

    const filteredUserDealerList = this.userDealerList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserDealerList;

  }

 

  // ruolo decrescente, poi descrizione
  public readonly sortByRole = (a: UserDealerListModel, b: UserDealerListModel) => {
    const roleA = this.parameterService.Roles.find(role => role.id === a.roleId)?.code || 0;
    const roleB = this.parameterService.Roles.find(role => role.id === b.roleId)?.code || 0;

    // Primo criterio: ordinamento decrescente su roleId
    if (roleB !== roleA) {
      return roleB - roleA;
    }

    // Secondo criterio: ordinamento ascendente su descrizione
    return a.descrizione.localeCompare(b.descrizione);
  };

  isPowerAdmin(item: UserDealerListModel) {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.DealerPowerAdmin);

    if (item.roleId == roles[0]?.id)
      return true;
    else
      return false;
  }


}
