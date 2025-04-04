import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { UserDataAdditionalModel, UserNavigationService } from '../../Utility/user-navigation.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { FinancialAdvisorListInModel, FinancialAdvisorListModel } from '../../ClientServer-Service/Account/Models/financial-advisor-model';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';

@Component({
  selector: 'app-financial-advisor-list-page',
  templateUrl: './financial-advisor-list-page.component.html',
  styleUrls: ['./financial-advisor-list-page.component.scss'],
  standalone: false
})
export class FinancialAdvisorListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public financialAdvisorList: FinancialAdvisorListModel[] | null = null;
  public btnImpersona: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {
    this.title = 'FinancialAdvisorListPage';

    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;

  }

  ionViewWillEnter() {
    let request: GenericRequest<FinancialAdvisorListInModel> = new GenericRequest<FinancialAdvisorListInModel>(FinancialAdvisorListInModel);
    this.accountService.FinancialAdvisorList(request).subscribe(res => {

      this.financialAdvisorList = res.data.financialAdvisorList;

    });
  }

  ngOnInit() {}

//handleButtonImpersonaClick(item: FinancialAdvisorListModel) {
  handleButtonImpersonaClick = (item: any) => {

    let request: GenericRequest<UserLoadInModel> = new GenericRequest<UserLoadInModel>(UserLoadInModel);
    request.data.id = item.idAspNetUsers;
    this.accountService.UserLoad(request).subscribe(usl => {
      if (usl.success) {

        let userDataAdditional: UserDataAdditionalModel = new UserDataAdditionalModel();
        userDataAdditional.gotoBackPage = "/financialadvisorlist";

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

