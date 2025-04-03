import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { DealerListInModel, DealerListModel } from '../../ClientServer-Service/Account/Models/dealer-list-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { UserDataAdditionalModel, UserNavigationService } from '../../Utility/user-navigation.service';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../Utility/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';

@Component({
  selector: 'app-dealer-list-page',
  templateUrl: './dealer-list-page.component.html',
  styleUrls: ['./dealer-list-page.component.scss'],
  standalone: false
})
export class DealerListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public dealerList: DealerListModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnImpersona: ButtonItem;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {
    this.title = 'DealerListPage';
    console.log('NavController instance:', this.navCtrl);

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;

  }

  ionViewWillEnter() {

    let request: GenericRequest<DealerListInModel> = new GenericRequest<DealerListInModel>(DealerListInModel);
    this.accountService.DealerList(request).subscribe(res => {

      this.dealerList = res.data.dealerList;

    });

    this.fabMenuService.fabMenuItem =   [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/dealeredit', {
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
        userDataAdditional.gotoBackPage = "/dealerlist";

        this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
        this.navCtrl.navigateForward('/home');
      }
      else {

      }
    });

  }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/dealeredit', {
      state: { id: item.idDealer }
    });
  }




  public Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

}
