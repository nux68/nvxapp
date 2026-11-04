import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocTipoModel, DocEmptyInModel, DocIdInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// Tipologie dei documenti (CompanyPowerAdmin): una tipologia usata non si elimina, si disattiva
@Component({
  selector: 'app-doc-type-list-page',
  templateUrl: './doc-type-list-page.component.html',
  styleUrls: ['./doc-type-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocTypeListPageComponent {

  public title: string;
  public searchText!: string;
  public items: DocTipoModel[] | null = null;
  public readonly columns: NvxListColumn<DocTipoModel>[];
  public readonly actions: NvxListAction<DocTipoModel>[];
  public readonly isDisabled = (x: DocTipoModel) => !x.active;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.typeList.title');
    const t = (k: string) => this.transloco.translate(k);
    this.columns = [
      { key: 'codice', title: t('documents.fields.code'), mobile: 'note' },
      { key: 'descrizione', title: t('documents.fields.description'), mobile: 'title' },
      { key: 'categoria', title: t('documents.fields.category'), value: x => t('documents.category.' + x.categoria) },
      { key: 'anniConservazione', title: t('documents.fields.retention'), align: 'end', value: x => x.anniConservazione ? String(x.anniConservazione) : '–' },
      { key: 'documenti', title: t('documents.fields.documents'), align: 'end' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: DocTipoModel) => this.navCtrl.navigateForward('/doctypeedit', { state: { item: { ...x }, setup: this.setup } });
    const btnDelete = userInterfaceService.Btn_Cancella;
    btnDelete.event = (x: DocTipoModel) => this.elimina(x);
    this.actions = [{ button: btnDelete, visible: (x: DocTipoModel) => x.active }, { button: btnEdit }];
  }

  private setup: any = null;

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('documents.pages.typeList.new'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/doctypeedit', { state: { item: new DocTipoModel(), setup: this.setup } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => {
      this.setup = res.data;
      this.items = res.success && res.data ? res.data.tipi : [];
    });
  }

  private async elimina(x: DocTipoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.typeList.confirmDelete')))
      return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = x.id;
    this.service.TipoDelete(request).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
