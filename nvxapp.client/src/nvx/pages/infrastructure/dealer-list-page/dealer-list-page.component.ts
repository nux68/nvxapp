import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { DealerListInModel, DealerListModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/dealer-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserDataAdditionalModel, UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { DeletionDialogService } from '../../../shared/components/infrastructure/deletion-dialog/deletion-dialog.service';
import { DeletionNodeType } from '../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-dealer-list-page',
  templateUrl: './dealer-list-page.component.html',
  styleUrls: ['./dealer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DealerListPageComponent {

  public title!: string;
  public searchText!: string;
  public dealerList: DealerListModel[] | null = null;
  public readonly columns: NvxListColumn<DealerListModel>[];
  public readonly actions: NvxListAction<DealerListModel>[];
  public readonly isDisabled = (item: DealerListModel) => item.disabled;
  public readonly sortByDescrizione = (a: DealerListModel, b: DealerListModel) => a.descrizione.localeCompare(b.descrizione);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  // opzioni della lista (nvx-list-options): Mostra disabilitati e Abilita cancellazioni (il pulsante Cancella)
  public showDisabled = false;
  public deleteEnabled = false;
  public btnImpersona: ButtonItem;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private transloco: TranslocoService,
              private deletionDialogService: DeletionDialogService,
              private authService: AuthService) {
    this.title = this.transloco.translate('pages.dealerList.title');
    this.columns = [{ key: 'descrizione', title: this.transloco.translate('pages.dealerList.columnDealer') }];
    console.log('NavController instance:', this.navCtrl);

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


  // eliminazione: il SuperUser (anche mentre impersona) o il PowerAdmin indicato in ANALISI_CANCELLAZIONE.md
  get canDelete(): boolean {
    return this.userNavigationService.LoginIsSuperUser;
  }

  handleButtonDeleteClick = async (item: any) => {
    if (await this.deletionDialogService.open(DeletionNodeType.Dealer, item.idDealer))
      this.ionViewWillEnter();
  }

}
