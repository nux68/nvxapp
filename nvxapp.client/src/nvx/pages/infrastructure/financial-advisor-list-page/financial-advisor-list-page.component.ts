import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { UserDataAdditionalModel, UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { FinancialAdvisorListInModel, FinancialAdvisorListModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/financial-advisor-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { DeletionDialogService } from '../../../shared/components/infrastructure/deletion-dialog/deletion-dialog.service';
import { DeletionNodeType } from '../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-financial-advisor-list-page',
  templateUrl: './financial-advisor-list-page.component.html',
  styleUrls: ['./financial-advisor-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FinancialAdvisorListPageComponent {

  public title!: string;
  public searchText!: string;
  public financialAdvisorList: FinancialAdvisorListModel[] | null = null;
  public readonly columns: NvxListColumn<FinancialAdvisorListModel>[];
  public readonly actions: NvxListAction<FinancialAdvisorListModel>[];
  public readonly isDisabled = (item: FinancialAdvisorListModel) => item.disabled;
  public readonly sortByDescrizione = (a: FinancialAdvisorListModel, b: FinancialAdvisorListModel) => a.descrizione.localeCompare(b.descrizione);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  // opzioni della lista (nvx-list-options): Mostra disabilitati e Abilita cancellazioni (il pulsante Cancella)
  public showDisabled = false;
  public deleteEnabled = false;
  public btnImpersona: ButtonItem;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    private userInterfaceService: UserInterfaceService,
    public fabMenuService: FabMenuService,
    private userNavigationService: UserNavigationService,
    private transloco: TranslocoService,
    private deletionDialogService: DeletionDialogService,
    private authService: AuthService) {

    this.title = this.transloco.translate('pages.financialAdvisorList.title');
    this.columns = [{ key: 'descrizione', title: this.transloco.translate('pages.financialAdvisorList.columnFinancialAdvisor') }];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;

    // cancellazione fisica (anteprima e conferma nel dialog; permessi controllati dal server)

    this.btnDelete = userInterfaceService.Btn_Cancella;

    this.btnDelete.event = this.handleButtonDeleteClick;


    this.actions = [{ button: this.btnDelete, visible: () => this.canDelete && this.deleteEnabled }, { button: this.btnImpersona }, { button: this.btnEdit }];

  }

  ionViewWillEnter() {
    let request: GenericRequest<FinancialAdvisorListInModel> = new GenericRequest<FinancialAdvisorListInModel>(FinancialAdvisorListInModel);
    this.accountService.FinancialAdvisorList(request).subscribe(res => {

      this.financialAdvisorList = res.data.financialAdvisorList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/financialadvisoredit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


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

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/financialadvisoredit', {
      state: { id: item.idFinancialAdvisor }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }



  // eliminazione: il SuperUser (anche mentre impersona) o il PowerAdmin indicato in ANALISI_INFRASTRUCTURE_CANCELLAZIONE.md
  get canDelete(): boolean {
    return this.userNavigationService.LoginIsSuperUser || this.authService.IsDealerPowerAdmin;
  }

  handleButtonDeleteClick = async (item: any) => {
    if (await this.deletionDialogService.open(DeletionNodeType.FinancialAdvisor, item.idFinancialAdvisor))
      this.ionViewWillEnter();
  }

}

