import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocCartellaModel, DocDipendenteModel, DocDocumentoListInModel, DocDocumentoListModel, DocEmptyInModel, DocIdInModel, DocPermessiModel, DocTipoModel, docSize } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

/*
 Archivio dei documenti dell'azienda (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md §7.1): filtri per cartella, tipologia, dipendente, anno,
 ricerca nel testo dei file; cestino. Ogni utente vede solo cio' che il server gli consente (documenti del personale
 solo con abilitazione). I documenti oltre la conservazione della tipologia sono segnalati, mai eliminati da soli.
 Colonna Firma: stato della firma se richiesta all'invio (firmato / firmati x su y / da firmare), altrimenti "non attiva".
*/
@Component({
  selector: 'app-doc-list-page',
  templateUrl: './doc-list-page.component.html',
  styleUrls: ['./doc-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocListPageComponent {

  public title: string;
  public searchText!: string;
  public documenti: DocDocumentoListModel[] | null = null;
  public troncato = false;
  public permessi = new DocPermessiModel();
  public tipi: DocTipoModel[] = [];
  public cartelle: DocCartellaModel[] = [];
  public dipendenti: DocDipendenteModel[] = [];
  public filtro = new DocDocumentoListInModel();
  public readonly columns: NvxListColumn<DocDocumentoListModel>[];
  public actions: NvxListAction<DocDocumentoListModel>[] = [];
  public readonly sort = (a: DocDocumentoListModel, b: DocDocumentoListModel) =>
    (b.dataDocumento ?? b.dataCaricamento ?? '').localeCompare(a.dataDocumento ?? a.dataCaricamento ?? '');
  private btnApri: ButtonItem;
  private btnScarica: ButtonItem;
  private btnRipristina: ButtonItem;
  private btnElimina: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.list.title');
    this.columns = [
      { key: 'titolo', title: this.transloco.translate('documents.fields.title'), mobile: 'title',
        value: d => d.titolo + (d.oltreConservazione ? ' ⏳' : '') + (d.stato === 0 ? ' (' + this.transloco.translate('documents.status.0') + ')' : '') },
      { key: 'tipo', title: this.transloco.translate('documents.fields.type') },
      { key: 'riferimento', title: this.transloco.translate('documents.fields.reference'), value: d => d.dipendente ?? d.cliente ?? d.cartella ?? '' },
      { key: 'competenza', title: this.transloco.translate('documents.fields.period'), mobile: 'note',
        value: d => d.anno ? (d.mese ? String(d.mese).padStart(2, '0') + '/' : '') + d.anno : (d.dataDocumento ?? '').substring(0, 10),
        sortValue: d => (d.anno ?? 0) * 100 + (d.mese ?? 0) },
      { key: 'dataScadenza', title: this.transloco.translate('documents.fields.expiry'), value: d => (d.dataScadenza ?? '').substring(0, 10) },
      { key: 'firma', title: this.transloco.translate('documents.fields.signature'), value: d => this.firma(d).testo, sortValue: d => this.firma(d).ordine },
      { key: 'dimensione', title: this.transloco.translate('documents.fields.size'), align: 'end', mobile: 'hidden', value: d => docSize(d.dimensione), sortValue: d => d.dimensione },
    ];
    this.btnApri = userInterfaceService.Btn_Apri;
    this.btnApri.event = (d: DocDocumentoListModel) => this.navCtrl.navigateForward('/docedit', { state: { id: d.id } });
    this.btnScarica = userInterfaceService.Btn_Export;
    this.btnScarica.text = this.transloco.translate('documents.buttons.download');
    this.btnScarica.event = (d: DocDocumentoListModel) => this.service.Download(d.id);
    this.btnRipristina = userInterfaceService.Btn_Riapri;
    this.btnRipristina.text = this.transloco.translate('documents.buttons.restore');
    this.btnRipristina.event = (d: DocDocumentoListModel) => this.azione(d, 'ripristina');
    this.btnElimina = userInterfaceService.Btn_Cancella;
    this.btnElimina.event = (d: DocDocumentoListModel) => this.azione(d, 'elimina');
  }

  // stato della firma per la colonna: testo (anche per ricerca), colore del badge e ordine (prima quelli da firmare)
  firma(d: DocDocumentoListModel): { testo: string; colore: string; ordine: number } {
    const t = (k: string, p?: object) => this.transloco.translate('documents.pages.list.' + k, p);
    if (!d.firmaDocumento)
      return { testo: t('signNotActive'), colore: 'medium', ordine: 4 };
    if (d.firmatari > 0 && d.firmati >= d.firmatari)
      return { testo: t('signedOn', { date: this.data(d.ultimaFirma) }), colore: 'success', ordine: 3 };
    if (d.firmati > 0)
      return { testo: t('signedPartial', { firmati: d.firmati, firmatari: d.firmatari }), colore: 'warning', ordine: 2 };
    return { testo: t('signPending'), colore: 'danger', ordine: 1 };
  }

  private data(iso: string | null): string {
    return iso ? iso.substring(8, 10) + '/' + iso.substring(5, 7) + '/' + iso.substring(0, 4) : '';
  }

  ionViewWillEnter() {
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => {
      if (!res.success || !res.data) return;
      this.permessi = res.data.permessi;
      this.tipi = res.data.tipi;
      this.cartelle = res.data.cartelle;
      this.dipendenti = res.data.dipendenti;
      const modifica = this.permessi.amministraArchivio || this.permessi.caricaPersonale;
      this.fabMenuService.fabMenuItem = modifica ? [
        new FabMenuItem(this.transloco.translate('documents.pages.list.upload'), 'cloud-upload-outline', () =>
          this.navCtrl.navigateForward('/docedit', { state: { id: 0, idDoc_Cartella: this.filtro.idDoc_Cartella, idDip_Anagrafica: this.filtro.idDip_Anagrafica } })),
        new FabMenuItem(this.transloco.translate(this.filtro.cestino ? 'documents.pages.list.archive' : 'documents.pages.list.trash'), 'trash-outline', () => this.toggleCestino()),
        ...(this.filtro.cestino ? [new FabMenuItem(this.transloco.translate('documents.pages.list.emptyTrash'), 'close-circle-outline', () => this.svuota())] : [])
      ] : [];
    });
    this.load();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.actions = this.filtro.cestino ? [{ button: this.btnElimina }, { button: this.btnRipristina }] : [{ button: this.btnScarica }, { button: this.btnApri }];
    const request = new GenericRequest<DocDocumentoListInModel>(DocDocumentoListInModel);
    request.data = { ...this.filtro, testo: this.filtro.testo?.trim() || null, anno: this.filtro.anno ? Number(this.filtro.anno) : null };
    this.service.List(request).subscribe(res => {
      this.documenti = res.success && res.data ? res.data.documenti : [];
      this.troncato = !!res.data?.troncato;
    });
  }

  toggleCestino() {
    this.filtro.cestino = !this.filtro.cestino;
    this.ionViewWillEnter();
  }

  private async svuota() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.list.confirmEmptyTrash')))
      return;
    this.service.SvuotaCestino().subscribe(() => this.load());
  }

  private async azione(d: DocDocumentoListModel, tipo: 'ripristina' | 'elimina') {
    if (tipo === 'elimina' && !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.list.confirmDelete')))
      return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = d.id;
    (tipo === 'ripristina' ? this.service.Ripristina(request) : this.service.Elimina(request)).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
