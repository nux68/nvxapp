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
import { UserFinancialAdvisorListModel, UserFinancialAdvisorListInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-financial-advisor-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';


@Component({
  selector: 'app-user-financial-advisor-list-page',
  templateUrl: './user-financial-advisor-list-page.component.html',
  styleUrls: ['./user-financial-advisor-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone:false
})
export class UserFinancialAdvisorListPageComponent {

  public title!: string;
  public searchText!: string;
  public userFinancialAdvisorList: UserFinancialAdvisorListModel[] | null = null;
  public readonly columns: NvxListColumn<UserFinancialAdvisorListModel>[] = [{ key: 'descrizione', title: this.transloco.translate('pages.userFinancialAdvisorList.columnUser') }];
  public readonly actions: NvxListAction<UserFinancialAdvisorListModel>[];
  public readonly isDisabled = (item: UserFinancialAdvisorListModel) => item.disabled;
  public readonly isHighlighted = (item: UserFinancialAdvisorListModel) => this.isPowerAdmin(item);
  public readonly iconOf = (item: UserFinancialAdvisorListModel) => this.isPowerAdmin(item) ? 'person-add-outline' : 'person-outline';
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.userFinancialAdvisorList.title');

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.actions = [{ button: this.btnEdit }];
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserFinancialAdvisorListInModel> = new GenericRequest<UserFinancialAdvisorListInModel>(UserFinancialAdvisorListInModel);
    this.accountService.UserFinancialAdvisorList(request).subscribe(res => {

      this.userFinancialAdvisorList = res.data.userFinancialAdvisorList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem(this.transloco.translate('pages.userFinancialAdvisorList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/userfinancialadvisoredit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/userfinancialadvisoredit', {
      state: { id: item.idUserFinancialAdvisor }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.FinancialAdvisorAdmin );

    const filteredUserFinancialAdvisorList = this.userFinancialAdvisorList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserFinancialAdvisorList;

  }

  getPowerAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.FinancialAdvisorPowerAdmin);

    const filteredUserFinancialAdvisorList = this.userFinancialAdvisorList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserFinancialAdvisorList;

  }

  

  // ruolo decrescente, poi descrizione
  public readonly sortByRole = (a: UserFinancialAdvisorListModel, b: UserFinancialAdvisorListModel) => {
    const roleA = this.parameterService.Roles.find(role => role.id === a.roleId)?.code || 0;
    const roleB = this.parameterService.Roles.find(role => role.id === b.roleId)?.code || 0;

    // Primo criterio: ordinamento decrescente su roleId
    if (roleB !== roleA) {
      return roleB - roleA;
    }

    // Secondo criterio: ordinamento ascendente su descrizione
    return a.descrizione.localeCompare(b.descrizione);
  };

  isPowerAdmin(item: UserFinancialAdvisorListModel) {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.FinancialAdvisorPowerAdmin);

    if (item.roleId == roles[0]?.id)
      return true;
    else
      return false;
  }

}
