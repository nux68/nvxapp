import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CompanyStorageListInModel, CompanyStorageModel, StoragePlanModel, bytesToMB } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

/*
 Spazio dei documenti delle aziende (ANALISI_DOCUMENTALE.md §5.3): SuperUser (tutte), centro (le sue), studio (le sue,
 in sola lettura). Cambiano pacchetto e limiti solo il SuperUser e il DealerPowerAdmin del centro (canManage dal server).
 Le aziende oltre la soglia di avviso o oltre quota sono evidenziate.
*/
@Component({
  selector: 'app-company-storage-list-page',
  templateUrl: './company-storage-list-page.component.html',
  styleUrls: ['./company-storage-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyStorageListPageComponent {

  public title: string;
  public searchText!: string;
  public items: CompanyStorageModel[] | null = null;
  private plans: StoragePlanModel[] = [];
  public readonly columns: NvxListColumn<CompanyStorageModel>[];
  public readonly actions: NvxListAction<CompanyStorageModel>[];
  public readonly sortByUso = (a: CompanyStorageModel, b: CompanyStorageModel) => b.usedPercent - a.usedPercent || a.azienda.localeCompare(b.azienda);
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private storageService: StorageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.companyStorageList.title');
    this.columns = [
      { key: 'azienda', title: this.transloco.translate('pages.companyStorageList.columnCompany'), mobile: 'title' },
      { key: 'studio', title: this.transloco.translate('pages.companyStorageList.columnStudio') },
      { key: 'pacchetto', title: this.transloco.translate('pages.companyStorageList.columnPlan') },
      { key: 'usedBytes', title: this.transloco.translate('pages.companyStorageList.columnUsed'), align: 'end',
        value: c => bytesToMB(c.usedBytes) + ' / ' + c.quotaMB.toLocaleString() + ' MB', sortValue: c => c.usedBytes },
      { key: 'usedPercent', title: this.transloco.translate('pages.companyStorageList.columnPercent'), align: 'end', mobile: 'note',
        value: c => c.usedPercent + '%' + (c.overQuota ? ' ⛔' : c.overWarning ? ' ⚠' : ''), sortValue: c => c.usedPercent },
      { key: 'fileCount', title: this.transloco.translate('pages.companyStorageList.columnFiles'), align: 'end', mobile: 'hidden' },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.actions = [{ button: this.btnEdit, visible: c => c.canManage }];
  }

  ionViewWillEnter() {
    this.storageService.CompanyStorageList(new GenericRequest<CompanyStorageListInModel>(CompanyStorageListInModel)).subscribe(res => {
      this.items = res.success && res.data ? res.data.companyStorage : [];
      this.plans = res.success && res.data ? res.data.plans : [];
    });
  }

  handleButtonEditClick = (item: CompanyStorageModel) => {
    this.navCtrl.navigateForward('/companystorageedit', { state: { companyStorage: { ...item }, plans: this.plans } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
