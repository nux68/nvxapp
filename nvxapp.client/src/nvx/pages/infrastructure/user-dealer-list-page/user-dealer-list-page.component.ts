import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Parameter/parameter.service';

import { UserLoadInModel } from '../../../ClientServer-Service/Account/Models/user-load-model';
import { UserCompanyListModel, UserCompanyListInModel } from '../../../ClientServer-Service/Account/Models/user-company-model';
import { UserDealerListModel, UserDealerListInModel } from '../../../ClientServer-Service/Account/Models/user-dealer-model';
import { RoleCode } from '../../../ClientServer-Service/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-dealer-list-page',
  templateUrl: './user-dealer-list-page.component.html',
  styleUrls: ['./user-dealer-list-page.component.scss'],
  standalone:false
})
export class UserDealerListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public userDealerList: UserDealerListModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'UserDealerListPage';

    
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserDealerListInModel> = new GenericRequest<UserDealerListInModel>(UserDealerListInModel);
    this.accountService.UserDealerList(request).subscribe(res => {

      this.userDealerList = res.data.userDealerList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/userdealeredit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }


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


}
