import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { UserCompanyListModel, UserCompanyListInModel } from '../../ClientServer-Service/Account/Models/uesr-company-list-model';
import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../Utility/user-navigation.service';

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

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    private userNavigationService: UserNavigationService) {
    this.title = 'UserCompanyListPage';
  }

  ngOnInit() {

    let request: GenericRequest<UserCompanyListInModel> = new GenericRequest<UserCompanyListInModel>(UserCompanyListInModel);
    this.accountService.UserCompanyList(request).subscribe(res => {

      this.userCompanyList = res.data.userCompanyList;

    });

  }

  handleButtonClick(item: UserCompanyListModel) {

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

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

}
