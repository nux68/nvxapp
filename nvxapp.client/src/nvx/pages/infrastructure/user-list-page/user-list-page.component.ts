import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { UserListInModel, UserListModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

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
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'Users';
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

 
  getAll() {
    const sortedUserList = this.userList.sort((a, b) => {
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

  isPowerAdmin(item: UserListModel) {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.PowerAdmin);

    if (item.roleId == roles[0].id)
      return true;
    else
      return false;
  }

}
