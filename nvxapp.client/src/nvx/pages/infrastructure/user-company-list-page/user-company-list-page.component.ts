import { Component, OnInit } from '@angular/core';
import { ModalController, NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';

import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserCompanyListModel, UserCompanyListInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { MainMenuService } from '../../../Utility/infrastructure/main-menu.service';
import { AddUserCompanyComponent } from '../../../shared/components/infrastructure/add-user-company/add-user-company.component';
import { UserCompanyEditModel, UserCompanyGetInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';

@Component({
  selector: 'app-user-company-list-page',
  templateUrl: './user-company-list-page.component.html',
  styleUrls: ['./user-company-list-page.component.scss'],
  standalone: false
})
export class UserCompanyListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public userCompanyList: UserCompanyListModel[] | null = null;
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
              private mainMenuService: MainMenuService,
    private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private modalCtrl: ModalController
            
            ) 
{

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

    const pageName = this.mainMenuService.RedefineNameOfPages('usercompanywizard');

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        // this.navCtrl.navigateForward('/' + pageName, {
        //   state: { id: 0 }
        // });
        this.openAddUserCompany();
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  async openAddUserCompany() {
    const modal = await this.modalCtrl.create({
      component: AddUserCompanyComponent,
      componentProps: {
        userCompanyEdit: new UserCompanyEditModel()
      }
    });

    await modal.present();

    const { data, role } = await modal.onDidDismiss();

    // Se l'utente ha confermato l'inserimento, recupera idAspNetUsers e naviga al wizard
    if (role === 'confirm' && data && data.idUserCompany) {
      const getRequest = new GenericRequest<UserCompanyGetInModel>(UserCompanyGetInModel);
      getRequest.data.id = data.idUserCompany;
      
      const pageName = this.mainMenuService.RedefineNameOfPages('usercompanywizard');
      this.navCtrl.navigateForward('/' + pageName, {
        state: { id: data.idAspNetUsers }
      });
      
    }
    // else {
    //   // Aggiorna la lista dopo la chiusura della modale senza conferma
    //   const request: GenericRequest<UserCompanyListInModel> = new GenericRequest<UserCompanyListInModel>(UserCompanyListInModel);
    //   this.accountService.UserCompanyList(request).subscribe(res => {
    //     this.userCompanyList = res.data.userCompanyList;
    //   });
    // }
  }


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
    const pageName = this.mainMenuService.RedefineNameOfPages('usercompanyedit');
    this.navCtrl.navigateForward('/' + pageName, {
      state: { id: item.idAspNetUsers }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyAdmin || role.code == RoleCode.CompanyPowerAdmin);

    //const filteredUserCompanyList = this.userCompanyList.filter(usr =>
    //  roles.some(role => role.id === usr.roleId)
    //);
    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      usr.roles.some(userRole => roles.some(role => role.name === userRole))
    );

    return filteredUserCompanyList;

  }

  getUser() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.User);

    //const filteredUserCompanyList = this.userCompanyList.filter(usr =>
    //  roles.some(role => role.id === usr.roleId)
    //);

    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      usr.roles.some(userRole => roles.some(role => role.name === userRole))
    );

    return filteredUserCompanyList;

  }




  getAll() {
    const sortedUserList = this.userCompanyList.sort((a, b) => {
      const roleA = this.parameterService.Roles.find(role => role.name === a.roles[0])?.code || 0;
      const roleB = this.parameterService.Roles.find(role => role.name === b.roles[0])?.code || 0;


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
    
    const adminRoles = this.parameterService.Roles
      .filter(role => role.code == RoleCode.CompanyAdmin || role.code == RoleCode.CompanyPowerAdmin)
      .map(x => x.name);

    return item.roles.some(role => adminRoles.includes(role));
  }


  isUser(item: UserCompanyListModel) {

    const adminRoles = this.parameterService.Roles
      .filter(role => role.code == RoleCode.User )
      .map(x => x.name);

    return item.roles.some(role => adminRoles.includes(role));
  }

}
