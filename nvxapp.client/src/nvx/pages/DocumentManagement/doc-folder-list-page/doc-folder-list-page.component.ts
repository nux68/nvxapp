import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocCartellaModel, DocEmptyInModel, DocIdInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// Cartelle dell'archivio (CompanyPowerAdmin): albero; si elimina solo una cartella vuota e non predefinita
@Component({
  selector: 'app-doc-folder-list-page',
  templateUrl: './doc-folder-list-page.component.html',
  styleUrls: ['./doc-folder-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocFolderListPageComponent {

  public title: string;
  public searchText!: string;
  public items: DocCartellaModel[] | null = null;
  public readonly columns: NvxListColumn<DocCartellaModel>[];
  public readonly actions: NvxListAction<DocCartellaModel>[];

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.folderList.title');
    const t = (k: string) => this.transloco.translate(k);
    this.columns = [
      { key: 'percorso', title: t('documents.fields.folder'), mobile: 'title' },
      { key: 'visibileUtenti', title: t('documents.fields.visibleToUsers'), align: 'center', value: x => x.visibileUtenti ? '✓' : '' },
      { key: 'documenti', title: t('documents.fields.documents'), align: 'end', mobile: 'note' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: DocCartellaModel) => this.navCtrl.navigateForward('/docfolderedit', { state: { item: { ...x }, setup: this.setup } });
    const btnDelete = userInterfaceService.Btn_Cancella;
    btnDelete.event = (x: DocCartellaModel) => this.elimina(x);
    this.actions = [{ button: btnDelete, visible: (x: DocCartellaModel) => !x.diSistema && x.documenti === 0 }, { button: btnEdit }];
  }

  private setup: any = null;

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('documents.pages.folderList.new'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/docfolderedit', { state: { item: new DocCartellaModel(), setup: this.setup } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => {
      this.setup = res.data;
      this.items = res.success && res.data ? res.data.cartelle : [];
    });
  }

  private async elimina(x: DocCartellaModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.folderList.confirmDelete')))
      return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = x.id;
    this.service.CartellaDelete(request).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
