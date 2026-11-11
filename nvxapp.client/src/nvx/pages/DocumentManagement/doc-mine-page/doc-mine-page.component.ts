import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocDocumentoListInModel, DocDocumentoListModel, DocFirmaInModel, DocIdInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// "I miei documenti" del dipendente (§7.3): documenti propri e di cui e' destinatario, presa visione con un clic, firma (FES)
@Component({
  selector: 'app-doc-mine-page',
  templateUrl: './doc-mine-page.component.html',
  styleUrls: ['./doc-mine-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocMinePageComponent {

  public title: string;
  public searchText!: string;
  public documenti: DocDocumentoListModel[] | null = null;
  public readonly columns: NvxListColumn<DocDocumentoListModel>[];
  public readonly actions: NvxListAction<DocDocumentoListModel>[];
  public readonly sort = (a: DocDocumentoListModel, b: DocDocumentoListModel) => (b.dataCaricamento ?? '').localeCompare(a.dataCaricamento ?? '');

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.mine.title');
    this.columns = [
      { key: 'titolo', title: this.transloco.translate('documents.fields.title'), mobile: 'title', value: d => (d.nuovo ? '● ' : '') + d.titolo },
      { key: 'tipo', title: this.transloco.translate('documents.fields.type') },
      { key: 'competenza', title: this.transloco.translate('documents.fields.period'), mobile: 'note',
        value: d => d.anno ? (d.mese ? String(d.mese).padStart(2, '0') + '/' : '') + d.anno : (d.dataCaricamento ?? '').substring(0, 10) },
      { key: 'presaVisione', title: this.transloco.translate('documents.fields.acknowledgement'),
        value: d => d.presaVisioneRichiesta ? (d.dataPresaVisione ? '✓ ' + d.dataPresaVisione.substring(0, 10) : this.transloco.translate('documents.pages.mine.toAcknowledge')) : '' },
      { key: 'firma', title: this.transloco.translate('documents.fields.signature'),
        value: d => d.firmaRichiesta ? (d.dataFirma ? '✓ ' + d.dataFirma.substring(0, 10) : this.transloco.translate('documents.pages.mine.toSign')) : '' },
    ];
    const btnScarica = userInterfaceService.Btn_Export;
    btnScarica.text = this.transloco.translate('documents.buttons.download');
    btnScarica.event = (d: DocDocumentoListModel) => this.service.Download(d.id).then(() => this.load());
    const btnPresa = userInterfaceService.Btn_Approva;
    btnPresa.text = this.transloco.translate('documents.buttons.acknowledge');
    btnPresa.event = (d: DocDocumentoListModel) => this.presaVisione(d);
    const btnFirma = userInterfaceService.Btn_Approva;
    btnFirma.text = this.transloco.translate('documents.buttons.sign');
    btnFirma.event = (d: DocDocumentoListModel) => this.firma(d);
    const btnApri = userInterfaceService.Btn_Apri;
    btnApri.event = (d: DocDocumentoListModel) => this.navCtrl.navigateForward('/docedit', { state: { id: d.id } });
    this.actions = [
      { button: btnFirma, visible: d => d.firmaRichiesta && !d.dataFirma },
      { button: btnPresa, visible: d => d.presaVisioneRichiesta && !d.dataPresaVisione && !(d.firmaRichiesta && !d.dataFirma) },
      { button: btnScarica }, { button: btnApri }];
  }

  ionViewWillEnter() {
    this.load();
  }

  load() {
    const request = new GenericRequest<DocDocumentoListInModel>(DocDocumentoListInModel);
    request.data.miei = true;
    this.service.List(request).subscribe(res => this.documenti = res.success && res.data ? res.data.documenti : []);
  }

  private async presaVisione(d: DocDocumentoListModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.mine.confirmAcknowledge', { title: d.titolo })))
      return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = d.id;
    this.service.PresaVisione(request).subscribe(() => this.load());
  }

  // firma della versione corrente: il server la accetta solo se il documento e' stato scaricato
  private async firma(d: DocDocumentoListModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.edit.confirmSign', { title: d.titolo, versione: d.versioneCorrente })))
      return;
    const request = new GenericRequest<DocFirmaInModel>(DocFirmaInModel);
    request.data.id = d.id;
    request.data.versione = d.versioneCorrente;
    this.service.Firma(request).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
