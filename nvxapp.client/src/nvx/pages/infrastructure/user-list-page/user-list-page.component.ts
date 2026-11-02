import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { DeletionDialogService } from '../../../shared/components/infrastructure/deletion-dialog/deletion-dialog.service';
import { DeletionNodeType } from '../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { UserListInModel, UserListModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-list-page',
  templateUrl: './user-list-page.component.html',
  styleUrls: ['./user-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone:false
})
export class UserListPageComponent {

  public title!: string;
  public searchText!: string;
  public userList: UserListModel[] | null = null;
  public readonly columns: NvxListColumn<UserListModel>[] = [{ key: 'descrizione', title: this.transloco.translate('pages.userList.columnUser') }];
  public readonly actions: NvxListAction<UserListModel>[];
  public readonly isHighlighted = (item: UserListModel) => this.isPowerAdmin(item);
  public readonly iconOf = (item: UserListModel) => this.isPowerAdmin(item) ? 'person-add-outline' : 'person-outline';
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  // opzioni della lista (nvx-list-options): Abilita cancellazioni (il pulsante Cancella)
  public deleteEnabled = false;

  constructor(private navCtrl: NavController,
    private accountService: AccountService,
    public fabMenuService: FabMenuService,
    private parameterService: ParameterService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private transloco: TranslocoService,
    private deletionDialogService: DeletionDialogService,
    private authService: AuthService) {

    this.title = this.transloco.translate('pages.userList.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    // cancellazione fisica (anteprima e conferma nel dialog; permessi controllati dal server)

    this.btnDelete = userInterfaceService.Btn_Cancella;

    this.btnDelete.event = this.handleButtonDeleteClick;


    this.actions = [{ button: this.btnDelete, visible: item => this.canDelete && this.deleteEnabled && !this.isAdminPrincipale(item) }, { button: this.btnEdit }];
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserListInModel> = new GenericRequest<UserListInModel>(UserListInModel);
    this.accountService.UserList(request).subscribe(res => {

      this.userList = res.data.userList;

    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem(this.transloco.translate('pages.userList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/useredit', {
          state: { id: '' }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/useredit', {
      state: { id: item.idAspNetUsers }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.Admin);

    const filtereduserList = this.userList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filtereduserList;

  }

  getPowerAdmin() {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.PowerAdmin);

    const filtereduserList = this.userList.filter(usr =>
      roles.some(role => role.id === usr.roleId)
    );

    return filtereduserList;

  }

 
  // ruolo decrescente, poi descrizione
  public readonly sortByRole = (a: UserListModel, b: UserListModel) => {
    const roleA = this.parameterService.Roles.find(role => role.id === a.roleId)?.code || 0;
    const roleB = this.parameterService.Roles.find(role => role.id === b.roleId)?.code || 0;

    // Primo criterio: ordinamento decrescente su roleId
    if (roleB !== roleA) {
      return roleB - roleA;
    }

    // Secondo criterio: ordinamento ascendente su descrizione
    return a.descrizione.localeCompare(b.descrizione);
  };

  isPowerAdmin(item: UserListModel) {

    const roles = this.parameterService.Roles.filter(role => role.code == RoleCode.PowerAdmin);

    if (item.roleId == roles[0]?.id)
      return true;
    else
      return false;
  }

  // PowerAdmin, DealerPowerAdmin, FinancialAdvisorPowerAdmin e CompanyPowerAdmin non si eliminano da soli (solo con il loro nodo)
  isAdminPrincipale(item: any): boolean {
    const codici = [RoleCode.PowerAdmin, RoleCode.DealerPowerAdmin, RoleCode.FinancialAdvisorPowerAdmin, RoleCode.CompanyPowerAdmin];
    const ruoli = this.parameterService.Roles.filter(role => codici.includes(role.code));
    return ruoli.some(x => x.id == item.roleId);
  }

  // eliminazione: il SuperUser (anche mentre impersona) o il PowerAdmin indicato in ANALISI_CANCELLAZIONE.md
  get canDelete(): boolean {
    return this.userNavigationService.LoginIsSuperUser;
  }

  handleButtonDeleteClick = async (item: any) => {
    if (await this.deletionDialogService.open(DeletionNodeType.User, null, item.idAspNetUsers))
      this.ionViewWillEnter();
  }

}
