import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Account/account.service';
import { CompanyListModel, CompanyListInModel } from '../../../ClientServer-Service/Account/Models/company-model';
import { UserLoadInModel } from '../../../ClientServer-Service/Account/Models/user-load-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/fab-menu.service';


@Component({
  selector: 'app-company-list-page',
  templateUrl: './company-list-page.component.html',
  styleUrls: ['./company-list-page.component.scss'],
  standalone: false
})
export class CompanyListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public companyList: CompanyListModel[] | null = null;
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.title = 'CompanyListPage';
    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

  }

  ionViewWillEnter() {

    let request: GenericRequest<CompanyListInModel> = new GenericRequest<CompanyListInModel>(CompanyListInModel);
    this.accountService.CompanyList(request).subscribe(res => {

      this.companyList = res.data.companyList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/companyedit', {
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
        userDataAdditional.gotoBackPage = "/companylist";

        this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
        this.navCtrl.navigateForward('/home');
      }
      else {

      }
    });

  }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/companyedit', {
      state: { id: item.idCompany }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }


}
