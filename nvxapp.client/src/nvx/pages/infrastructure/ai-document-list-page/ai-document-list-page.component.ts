import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import {
  DocumentDeleteInModel, DocumentListInModel, DocumentListModel, DocumentScopeInModel, DocumentScopeModel, DocumentSection
} from '../../../ClientServer-Service/Infrastructure/Documentation/Models/document-model';
import { DocumentService } from '../../../ClientServer-Service/Infrastructure/Documentation/document.service';
import { TicketLevel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Documentazione per le risposte automatiche AI ai ticket (SuperUser, centro, studio, azienda).
 I livelli li decide il server dal ruolo e dal token: se l'utente ne gestisce piu' di uno sceglie quale vedere.
 Modificano SuperUser e PowerAdmin; gli Admin vedono l'elenco e il dettaglio in sola lettura.
*/
@Component({
  selector: 'app-ai-document-list-page',
  templateUrl: './ai-document-list-page.component.html',
  styleUrls: ['./ai-document-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AiDocumentListPageComponent {

  public title: string;
  public searchText!: string;
  public scopes: DocumentScopeModel[] = [];
  public level: TicketLevel | null = null;
  public loaded = false;
  public documentList: DocumentListModel[] | null = null;
  public readonly columns: NvxListColumn<DocumentListModel>[];
  public readonly actions: NvxListAction<DocumentListModel>[];
  public readonly isDisabled = (item: DocumentListModel) => !item.active;
  public readonly sortByOrdine = (a: DocumentListModel, b: DocumentListModel) => a.ordine - b.ordine || a.titolo.localeCompare(b.titolo);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private documentService: DocumentService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.aiDocumentList.title');
    this.columns = [
      { key: 'titolo', title: this.transloco.translate('pages.aiDocumentList.columnTitle'), mobile: 'title' },
      { key: 'idTicketType', title: this.transloco.translate('pages.aiDocumentList.columnTypes'), sortable: false,
        value: d => this.typeNames(d) },
      { key: 'includeSubLevels', title: this.transloco.translate('pages.aiDocumentList.columnSubLevels'), width: '110px', mobile: 'hidden',
        value: d => this.transloco.translate(d.includeSubLevels ? 'pages.aiDocumentList.yes' : 'pages.aiDocumentList.no') },
      { key: 'contenutoLength', title: this.transloco.translate('pages.aiDocumentList.columnLength'), width: '110px', align: 'end', mobile: 'note',
        value: d => this.transloco.translate('pages.aiDocumentList.characters', { n: d.contenutoLength.toLocaleString() }), sortValue: d => d.contenutoLength },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [
      { button: this.btnEdit },
      { button: this.btnDelete, visible: () => this.scope?.canEdit ?? false },
    ];
  }

  get scope(): DocumentScopeModel | null {
    return this.scopes.find(s => s.level === this.level) ?? null;
  }

  ionViewWillEnter() {
    const request = new GenericRequest<DocumentScopeInModel>(DocumentScopeInModel);
    request.data.section = DocumentSection.TicketAi;
    this.documentService.DocumentScope(request).subscribe(res => {
      this.scopes = res.success && res.data ? res.data.scope : [];
      this.loaded = true;
      // al ritorno dal dettaglio resta il livello scelto
      if (this.level === null || !this.scopes.some(s => s.level === this.level))
        this.level = this.scopes.length > 0 ? this.scopes[0].level : null;
      this.loadList();
    });
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  loadList() {
    this.fabMenuService.fabMenuItem = this.scope?.canEdit
      ? [new FabMenuItem(this.transloco.translate('pages.aiDocumentList.newDocument'), 'add-circle-outline', () => {
          this.navCtrl.navigateForward('/aidocumentedit', { state: { id: 0, level: this.level } });
        })]
      : [];

    if (this.level === null) {
      this.documentList = [];
      return;
    }

    const request = new GenericRequest<DocumentListInModel>(DocumentListInModel);
    request.data.section = DocumentSection.TicketAi;
    request.data.level = this.level;
    this.documentService.DocumentList(request).subscribe(res => {
      this.documentList = res.success && res.data ? res.data.document : [];
    });
  }

  handleButtonEditClick = (item: DocumentListModel) => {
    this.navCtrl.navigateForward('/aidocumentedit', {
      state: { id: item.id, level: this.level }
    });
  }

  handleButtonDeleteClick = async (item: DocumentListModel) => {
    const confirmed = await this.collectionDialogService.ConfirmCancelDialog(
      this.transloco.translate('pages.aiDocumentList.deleteConfirm', { title: item.titolo }));
    if (!confirmed)
      return;

    const request = new GenericRequest<DocumentDeleteInModel>(DocumentDeleteInModel);
    request.data.id = item.id;
    this.documentService.DocumentDelete(request).subscribe(res => {
      if (res.success)
        this.loadList();
    });
  }

  // tipi di richiesta collegati; nessuno = tutti
  private typeNames(d: DocumentListModel): string {
    if (d.idTicketType.length === 0)
      return this.transloco.translate('pages.aiDocumentList.allTypes');
    const types = this.scope?.ticketType ?? [];
    return d.idTicketType.map(id => types.find(t => t.id === id)?.descrizione ?? '#' + id).join(', ');
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
