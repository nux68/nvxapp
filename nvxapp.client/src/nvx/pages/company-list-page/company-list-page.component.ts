import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { CompanyListModel, CompanyListInModel } from '../../ClientServer-Service/Account/Models/company-list-model';
import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../Utility/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';


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

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.title = 'CompanyListPage';
    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;

  }

  ionViewWillEnter() {

    let request: GenericRequest<CompanyListInModel> = new GenericRequest<CompanyListInModel>(CompanyListInModel);
    this.accountService.CompanyList(request).subscribe(res => {

      this.companyList = res.data.companyList;

    });

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

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }


}
