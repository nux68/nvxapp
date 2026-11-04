import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { StoragePlanDeleteInModel, StoragePlanModel, StorageSettingsGetInModel, StorageSettingsModel, bytesToMB } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

/*
 Pacchetti di spazio per i documenti delle aziende (SuperUser, ANALISI_DOCUMENTALE.md §5.3).
 Lo spazio di un'azienda e' sempre quello del suo pacchetto; uno e' il predefinito. Un pacchetto usato da aziende
 non si elimina: il server lo disattiva. Dal menu della pagina: valori predefiniti (dimensione massima, soglia).
*/
@Component({
  selector: 'app-storage-plan-list-page',
  templateUrl: './storage-plan-list-page.component.html',
  styleUrls: ['./storage-plan-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class StoragePlanListPageComponent {

  public title: string;
  public searchText!: string;
  public plans: StoragePlanModel[] | null = null;
  public settings: StorageSettingsModel | null = null;
  public totalUsed = '';
  public readonly columns: NvxListColumn<StoragePlanModel>[];
  public readonly actions: NvxListAction<StoragePlanModel>[];
  public readonly isDisabled = (item: StoragePlanModel) => !item.active;
  public readonly sortByOrdine = (a: StoragePlanModel, b: StoragePlanModel) => a.ordine - b.ordine || a.quotaMB - b.quotaMB;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private storageService: StorageService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.storagePlanList.title');
    this.columns = [
      { key: 'descrizione', title: this.transloco.translate('pages.storagePlanList.columnDescription'), mobile: 'title' },
      { key: 'quotaMB', title: this.transloco.translate('pages.storagePlanList.columnQuota'), align: 'end',
        value: p => bytesToMB(p.quotaMB * 1024 * 1024) + ' MB', sortValue: p => p.quotaMB },
      { key: 'maxFileMB', title: this.transloco.translate('pages.storagePlanList.columnMaxFile'), align: 'end',
        value: p => p.maxFileMB ? p.maxFileMB + ' MB' : this.transloco.translate('pages.storagePlanList.defaultValue') },
      { key: 'predefinito', title: this.transloco.translate('pages.storagePlanList.columnDefault'), align: 'center',
        value: p => p.predefinito ? '✓' : '' },
      { key: 'aziende', title: this.transloco.translate('pages.storagePlanList.columnCompanies'), align: 'end', mobile: 'note' },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [{ button: this.btnDelete, visible: p => !p.predefinito && p.active }, { button: this.btnEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('pages.storagePlanList.newPlan'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/storageplanedit', { state: { plan: new StoragePlanModel() } });
      }),
      new FabMenuItem(this.transloco.translate('pages.storagePlanList.settings'), 'options-outline', () => {
        this.navCtrl.navigateForward('/storagesettings');
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  private load() {
    this.storageService.StorageSettingsGet(new GenericRequest<StorageSettingsGetInModel>(StorageSettingsGetInModel)).subscribe(res => {
      this.plans = res.success && res.data ? res.data.plans : [];
      this.settings = res.success && res.data ? res.data.settings : null;
      this.totalUsed = res.success && res.data ? bytesToMB(res.data.totalUsedBytes) : '';
    });
  }

  handleButtonEditClick = (item: StoragePlanModel) => {
    this.navCtrl.navigateForward('/storageplanedit', { state: { plan: { ...item } } });
  }

  handleButtonDeleteClick = async (item: StoragePlanModel) => {
    const message = item.aziende > 0
      ? this.transloco.translate('pages.storagePlanList.confirmDeactivate', { companies: item.aziende })
      : this.transloco.translate('pages.storagePlanList.confirmDelete');
    if (!await this.collectionDialogService.ConfirmCancelDialog(message))
      return;
    const request = new GenericRequest<StoragePlanDeleteInModel>(StoragePlanDeleteInModel);
    request.data.id = item.id;
    this.storageService.StoragePlanDelete(request).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
