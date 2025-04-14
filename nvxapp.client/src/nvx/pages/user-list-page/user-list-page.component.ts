import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../Utility/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../Utility/fab-menu.service';
import { ParameterService } from '../../ClientServer-Service/Parameter/parameter.service';

import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { UserCompanyListModel, UserCompanyListInModel } from '../../ClientServer-Service/Account/Models/user-company-model';

@Component({
  selector: 'app-user-list-page',
  templateUrl: './user-list-page.component.html',
  styleUrls: ['./user-list-page.component.scss'],
  standalone:false
})
export class UserListPageComponent  implements OnInit {

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

    this.title = 'UserListPage';

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

  ngOnInit() { }

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

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == 10 || role.code == 11);

    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserCompanyList;

  }

  getUser() {

    const roles = this.parameterService.Roles.filter(role => role.code == 0);

    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserCompanyList;

  }


}
