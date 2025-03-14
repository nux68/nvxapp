import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { DealerListInModel, DealerListModel } from '../../ClientServer-Service/Account/Models/dealer-list-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { UserDataAdditionalModel, UserNavigationService } from '../../Utility/user-navigation.service';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-dealer-list-page',
  templateUrl: './dealer-list-page.component.html',
  styleUrls: ['./dealer-list-page.component.scss'],
  standalone: false
})
export class DealerListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public dealerList: DealerListModel[] | null=null;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              private userNavigationService: UserNavigationService) {
    this.title = 'DealerListPage';
  }

  ionViewWillEnter() {

    let request: GenericRequest<DealerListInModel> = new GenericRequest<DealerListInModel>(DealerListInModel);
    this.accountService.DealerList(request).subscribe(res => {

      this.dealerList = res.data.dealerList;

    });

  }

  ngOnInit() {}

  handleButtonClick(item: DealerListModel) {

    let request: GenericRequest<UserLoadInModel> = new GenericRequest<UserLoadInModel>(UserLoadInModel);
    request.data.id = item.idAspNetUsers;
    this.accountService.UserLoad(request).subscribe(usl => {
      if (usl.success) {

        let userDataAdditional: UserDataAdditionalModel = new UserDataAdditionalModel();
        userDataAdditional.gotoBackPage = "/dealerlist";

        this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
        this.navCtrl.navigateForward('/home');
      }
      else {

      }
    });

  }

  public Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

}
