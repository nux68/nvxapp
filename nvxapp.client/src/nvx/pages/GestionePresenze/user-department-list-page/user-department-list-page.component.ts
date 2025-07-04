// Copia e adatta UserCompanyListPageComponent come UserDepartmentListPageComponent
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
import { Observable, forkJoin, map, catchError, throwError } from 'rxjs';
import { Az_Sedi_GetAll_InModel, Az_SediModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediReparto_Get4Admin_InModel, Az_SediRepartoModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Az_SediRepartoUser_GetAll_Period_InModel, Az_SediRepartoUserModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { AzSediRepartoUserServiceService } from '../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/az-sedi-reparto-user-service.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { MainMenuService } from '../../../Utility/infrastructure/main-menu.service';
@Component({
  selector: 'app-user-department-list-page',
  templateUrl: './user-department-list-page.component.html',
  styleUrls: ['./user-department-list-page.component.scss'],
  standalone: false
})
export class UserDepartmentListPageComponent implements OnInit {
  public title!: string;
  public searchText!: string;
  public userCompanyList: UserCompanyListModel[] | null = null;
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;
  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private azSediService: AzSediService,
              private azSediRepartoService: AzSediRepartoService,
              private azSediRepartoUserServiceService: AzSediRepartoUserServiceService,
              private parameterService: ParameterService,
              private mainMenuService: MainMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService)
  {

    this.title = 'Utenti';
    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }


  public userCompanyList_TMP: UserCompanyListModel[] | null = null;
  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];

  private Load_Init(): Observable<boolean> {

    let req_UserCompany: GenericRequest<UserCompanyListInModel> = new GenericRequest<UserCompanyListInModel>(UserCompanyListInModel);
    const req_UserCompanyObservable$ = this.accountService.UserCompanyList(req_UserCompany);

    let req_AzSedi: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    const azSediResultObservable$ = this.azSediService.GetAll(req_AzSedi);

    let req_AzSediRep: GenericRequest<Az_SediReparto_Get4Admin_InModel> = new GenericRequest<Az_SediReparto_Get4Admin_InModel>(Az_SediReparto_Get4Admin_InModel);
    const azSediRepartoResultObservable$ = this.azSediRepartoService.Get4Admin(req_AzSediRep);

    return forkJoin({
      res_UserCompany: req_UserCompanyObservable$,
      res_AzSedi: azSediResultObservable$,
      req_AzSediRep: azSediRepartoResultObservable$
    }).pipe(
      map(results => {
        this.userCompanyList_TMP = results.res_UserCompany?.data?.userCompanyList || [];
        this.az_SediList = results.res_AzSedi?.data?.az_Sedi || [];
        this.az_SediRepartoList = results.req_AzSediRep?.data?.az_SediReparto || [];

        return true;
      }),
      catchError(error => {
        console.error("SediRepartoUserNavigation Error fetching data.", error);
        this.userCompanyList = [];
        this.userCompanyList_TMP = [];
        this.az_SediList = [];
        this.az_SediRepartoList = [];
        return throwError(() => new Error('SediRepartoUserNavigation Failed to load data'));
      })
    );
  }


  ionViewWillEnter() {

    this.Load_Init().subscribe(res => {

      const idAz_SediReparto = this.az_SediRepartoList.map(x => x.id);

      let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
      request.data.idAz_SediReparto = idAz_SediReparto;

      this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
        next: res => {
          var repUser = res.data?.az_RepartoUser || [];
          var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);
          this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers) && x.userInDepartment == true);

          var idAspNetUsersInRep = this.az_SediRepartoUserList.map(x=> x.idAspNetUsers);

          this.userCompanyList = this.userCompanyList_TMP.filter(x => idAspNetUsersInRep.includes(x.idAspNetUsers));
          
        },
        error: err => {
          console.error("Error loading users for reparti:", err);
        }
      });



    });

    const pageName = this.mainMenuService.RedefineNameOfPages('usercompanyedit');


    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/' + pageName, {
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
    });
  }
  handleButtonEditClick = (item: any) => {
    const pageName = this.mainMenuService.RedefineNameOfPages('usercompanyedit');
    this.navCtrl.navigateForward('/' + pageName, {
      state: { id: item.idUserCompany }
    });
  }
  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
  getAdmin() {
    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyAdmin || role.code == RoleCode.CompanyPowerAdmin);
    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      usr.roles.some(userRole => roles.some(role => role.name === userRole))
    );
    return filteredUserCompanyList;
  }
  getUser() {
    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.User);
    const filteredUserCompanyList = this.userCompanyList.filter(usr =>
      usr.roles.some(userRole => roles.some(role => role.name === userRole))
    );
    return filteredUserCompanyList;
  }
  getAll() {
    const sortedUserList = this.userCompanyList.sort((a, b) => {
      const roleA = this.parameterService.Roles.find(role => role.name === a.roles[0])?.code || 0;
      const roleB = this.parameterService.Roles.find(role => role.name === b.roles[0])?.code || 0;
      if (roleB !== roleA) {
        return roleB - roleA;
      }
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
