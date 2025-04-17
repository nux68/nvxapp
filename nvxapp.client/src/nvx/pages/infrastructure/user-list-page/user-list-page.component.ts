import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Parameter/parameter.service';

import { UserLoadInModel } from '../../../ClientServer-Service/Account/Models/user-load-model';
import { UserListInModel, UserListModel } from '../../../ClientServer-Service/Account/Models/user-model';
import { RoleCode } from '../../../ClientServer-Service/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-list-page',
  templateUrl: './user-list-page.component.html',
  styleUrls: ['./user-list-page.component.scss'],
  standalone:false
})
export class UserListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public userList: UserListModel[] | null = null;
  //public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'UserListPage';

    //this.btnImpersona = userInterfaceService.Btn_Impersona;
    //this.btnImpersona.event = this.handleButtonImpersonaClick;
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserListInModel> = new GenericRequest<UserListInModel>(UserListInModel);
    this.accountService.UserList(request).subscribe(res => {

      this.userList = res.data.userList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/useredit', {
          state: { id: '' }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  //handleButtonImpersonaClick = (item: any) => {

  //  let request: GenericRequest<UserLoadInModel> = new GenericRequest<UserLoadInModel>(UserLoadInModel);
  //  request.data.id = item.idAspNetUsers;
  //  this.accountService.UserLoad(request).subscribe(usl => {
  //    if (usl.success) {

  //      let userDataAdditional: UserDataAdditionalModel = new UserDataAdditionalModel();
  //      userDataAdditional.gotoBackPage = "/userlist";

  //      this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
  //      this.navCtrl.navigateForward('/home');
  //    }
  //    else {

  //    }
  //  });

  //}

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/useredit', {
      state: { id: item.idAspNetUsers }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.Admin);

    const filtereduserList = this.userList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filtereduserList;

  }

  getPowerAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.PowerAdmin);

    const filtereduserList = this.userList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filtereduserList;

  }


}
