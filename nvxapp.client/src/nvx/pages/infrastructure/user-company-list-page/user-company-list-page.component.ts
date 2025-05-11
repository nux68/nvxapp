import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';

import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserCompanyListModel, UserCompanyListInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-company-list-page',
  templateUrl: './user-company-list-page.component.html',
  styleUrls: ['./user-company-list-page.component.scss'],
  standalone: false
})
export class UserCompanyListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public userCompanyList: UserCompanyListModel[] | null = null;
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private parameterService: ParameterService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Users';

    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserCompanyListInModel> = new GenericRequest<UserCompanyListInModel>(UserCompanyListInModel);
    this.accountService.UserCompanyList(request).subscribe(res => {

      this.userCompanyList = res.data.userCompanyList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/usercompanyedit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() {}

  handleButtonImpersonaClick = (item: any) => {

    let request: GenericRequest<UserLoadInModel> = new GenericRequest<UserLoadInModel>(UserLoadInModel);
    request.data.id = item.idAspNetUsers;
    this.accountService.UserLoad(request).subscribe(usl => {
      if (usl.success) {

        let userDataAdditional: UserDataAdditionalModel = new UserDataAdditionalModel();
        userDataAdditional.gotoBackPage = "/usercompanylist";

        this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
        this.navCtrl.navigateForward('/home');
      }
      else {

      }
    });

  }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/usercompanyedit', {
      state: { id: item.idUserCompany }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getUser() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.User);

    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserCompanyList;

  }

  getAll() {
    const sortedUserList = this.userCompanyList.sort((a, b) => {
      const roleA = this.parameterService.Roles.find(role => role.id === a.roleId)?.code || 0;
      const roleB = this.parameterService.Roles.find(role => role.id === b.roleId)?.code || 0;

      // Primo criterio: ordinamento decrescente su roleId
      if (roleB !== roleA) {
        return roleB - roleA;
      }

      // Secondo criterio: ordinamento ascendente su descrizione
      return a.descrizione.localeCompare(b.descrizione);
    });
    return sortedUserList;
  }

  isAdmin(item: UserCompanyListModel) {


    const rolesAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyAdmin);

    const rolesPowerAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyPowerAdmin);

    if (item.roles.includes(rolesAdmin.name) || item.roles.includes(rolesPowerAdmin.name)) {
      return true;
    }
    return false;

  }

  isUser(item: UserCompanyListModel) {

 

    const rolesUser = this.parameterService.Roles.find(role => role.code == RoleCode.User);

   

    if (item.roles.includes(rolesUser.name) ) {
      return true;
    }
    return false;

  }

}
