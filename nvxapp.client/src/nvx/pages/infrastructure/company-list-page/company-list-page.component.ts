import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ModalController, NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { CompanyListModel, CompanyListInModel, CompanyEditModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/company-model';
import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService, UserDataAdditionalModel } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { DeletionDialogService } from '../../../shared/components/infrastructure/deletion-dialog/deletion-dialog.service';
import { DeletionNodeType } from '../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { MainMenuService } from '../../../Utility/infrastructure/main-menu.service';
import { AddCompanyComponent } from '../../../shared/components/infrastructure/add-company/add-company.component';
import { TranslocoService } from '@jsverse/transloco';


@Component({
  selector: 'app-company-list-page',
  templateUrl: './company-list-page.component.html',
  styleUrls: ['./company-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyListPageComponent {

  public title!: string;
  public searchText!: string;
  public companyList: CompanyListModel[] | null = null;
  public readonly columns: NvxListColumn<CompanyListModel>[];
  public readonly actions: NvxListAction<CompanyListModel>[];
  public readonly isDisabled = (item: CompanyListModel) => item.disabled;
  public readonly sortByDescrizione = (a: CompanyListModel, b: CompanyListModel) => a.descrizione.localeCompare(b.descrizione);
  public btnImpersona: ButtonItem;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private accountService: AccountService,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private mainMenuService: MainMenuService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService,
              private deletionDialogService: DeletionDialogService,
              private authService: AuthService) {

    this.title = this.transloco.translate('pages.companyList.title');
    this.columns = [{ key: 'descrizione', title: this.transloco.translate('pages.companyList.columnCompany') }];
    this.btnImpersona = userInterfaceService.Btn_Impersona;
    this.btnImpersona.event = this.handleButtonImpersonaClick;
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    // cancellazione fisica (anteprima e conferma nel dialog; permessi controllati dal server)

    this.btnDelete = userInterfaceService.Btn_Cancella;

    this.btnDelete.event = this.handleButtonDeleteClick;


    this.actions = [{ button: this.btnDelete, visible: () => this.canDelete }, { button: this.btnImpersona }, { button: this.btnEdit }];

  }

  ionViewWillEnter() {

    let request: GenericRequest<CompanyListInModel> = new GenericRequest<CompanyListInModel>(CompanyListInModel);
    this.accountService.CompanyList(request).subscribe(res => {

      this.companyList = res.data.companyList;

    });

    const pageName = this.mainMenuService.RedefineNameOfPages('companywizard');

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.openAddCompany();
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  async openAddCompany() {
    const modal = await this.modalCtrl.create({
      component: AddCompanyComponent,
      componentProps: {
        companyEdit: new CompanyEditModel()
      }
    });

    await modal.present();

    const { data, role } = await modal.onDidDismiss();

    // Se l'utente ha confermato l'inserimento, recupera idCompany e naviga al wizard
    if (role === 'confirm' && data && data.idCompany) {
      const pageName = this.mainMenuService.RedefineNameOfPages('companywizard');
      this.navCtrl.navigateForward('/' + pageName, {
        state: { id: data.idCompany }
      });
    }
  }

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




  // eliminazione: il SuperUser (anche mentre impersona) o il PowerAdmin indicato in ANALISI_CANCELLAZIONE.md
  get canDelete(): boolean {
    return this.userNavigationService.LoginIsSuperUser || this.authService.IsFinancialAdvisorPowerAdmin;
  }

  handleButtonDeleteClick = async (item: any) => {
    if (await this.deletionDialogService.open(DeletionNodeType.Company, item.idCompany))
      this.ionViewWillEnter();
  }

}
