import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocDipendenteModel, DocEmptyInModel, DocIdInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// Dipendenti (destinatari dei documenti del personale), anche senza account: chi carica i documenti del personale
@Component({
  selector: 'app-doc-employee-list-page',
  templateUrl: './doc-employee-list-page.component.html',
  styleUrls: ['./doc-employee-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocEmployeeListPageComponent {

  public title: string;
  public searchText!: string;
  public items: DocDipendenteModel[] | null = null;
  public readonly columns: NvxListColumn<DocDipendenteModel>[];
  public readonly actions: NvxListAction<DocDipendenteModel>[];
  public readonly isDisabled = (x: DocDipendenteModel) => !x.attivo;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.employeeList.title');
    const t = (k: string) => this.transloco.translate(k);
    this.columns = [
      { key: 'nominativo', title: t('documents.fields.employee'), mobile: 'title' },
      { key: 'codiceFiscale', title: t('documents.fields.taxCode') },
      { key: 'matricola', title: t('documents.fields.employeeNumber'), mobile: 'note' },
      { key: 'haUtente', title: t('documents.fields.hasAccount'), align: 'center', value: x => x.haUtente ? '✓' : '' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: DocDipendenteModel) => this.navCtrl.navigateForward('/docemployeeedit', { state: { item: { ...x }, setup: this.setup } });
    this.actions = [{ button: btnEdit }];
  }

  private setup: any = null;

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('documents.pages.employeeList.new'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/docemployeeedit', { state: { item: new DocDipendenteModel(), setup: this.setup } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => {
      this.setup = res.data;
      this.items = res.success && res.data ? res.data.dipendenti : [];
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
