import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../Utility/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../Utility/fab-menu.service';
import { ParameterService } from '../../ClientServer-Service/Parameter/parameter.service';

import { UserLoadInModel } from '../../ClientServer-Service/Account/Models/user-load-model';
import { UserFinancialAdvisorListModel, UserFinancialAdvisorListInModel } from '../../ClientServer-Service/Account/Models/user-financial-advisor-model';


@Component({
  selector: 'app-user-financial-advisor-list-page',
  templateUrl: './user-financial-advisor-list-page.component.html',
  styleUrls: ['./user-financial-advisor-list-page.component.scss'],
  standalone:false
})
export class UserFinancialAdvisorListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public userFinancialAdvisorList: UserFinancialAdvisorListModel[] | null = null;
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'UserFinancialAdvisorListPage';

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserFinancialAdvisorListInModel> = new GenericRequest<UserFinancialAdvisorListInModel>(UserFinancialAdvisorListInModel);
    this.accountService.UserFinancialAdvisorList(request).subscribe(res => {

      this.userFinancialAdvisorList = res.data.userFinancialAdvisorList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/userfinancialadvisoredit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/userfinancialadvisoredit', {
      state: { id: item.idUserFinancialAdvisor }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == 100 );

    const filteredUserFinancialAdvisorList = this.userFinancialAdvisorList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserFinancialAdvisorList;

  }

  getPowerAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == 101);

    const filteredUserFinancialAdvisorList = this.userFinancialAdvisorList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filteredUserFinancialAdvisorList;

  }


}
