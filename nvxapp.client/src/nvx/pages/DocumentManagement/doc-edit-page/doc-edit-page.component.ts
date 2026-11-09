import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocCartellaModel, DocCategoria, DocClienteModel, DocDettaglioModel, DocDipendenteModel, DocDocumentoEditModel, DocDocumentoPutInModel, DocDocumentoUploadMultiploModel, DocEmptyInModel, DocIdInModel, DocPermessiModel, DocTipoInvio, DocTipoModel, docDateInput, docSize } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';
import { DipSelectorItem, DipSelectorModalComponent, DipSelectorResult } from '../../../shared/components/infrastructure/dip-selector-modal/dip-selector-modal.component';

/*
 Documento: nuovo (dati + file) o esistente (dati, versioni, destinatari e prese visione, cestino).
 I campi dipendente/cliente dipendono dalla categoria della tipologia; competenza e scadenza dalla tipologia.
 Chi non puo' modificare vede i dati in sola lettura, scarica il file e conferma la presa visione.
*/
@Component({
  selector: 'app-doc-edit-page',
  templateUrl: './doc-edit-page.component.html',
  styleUrls: ['./doc-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocEditPageComponent extends BasePageConfirmCancelComponent<DocDocumentoEditModel> {

  public permessi = new DocPermessiModel();
  public tipi: DocTipoModel[] = [];
  public cartelle: DocCartellaModel[] = [];
  public dipendenti: DocDipendenteModel[] = [];
  public clienti: DocClienteModel[] = [];
  public dettaglio: DocDettaglioModel | null = null;
  public file: File | null = null;
  public readonly size = docSize;
  public readonly Categoria = DocCategoria;

  // dipendenti scelti per un documento del personale (uno o piu' se la tipologia e' a invio multiplo)
  public dipendentiSelezionati: number[] = [];
  // esito dell'invio multiplo (documenti creati ed eventuali errori)
  private multiploErrori: string[] = [];
  private multiploCreati = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.edit.title'); }
  get Nuovo(): boolean { return !this._editModel?.id; }
  get Tipo(): DocTipoModel | undefined { return this.tipi.find(t => t.id === this._editForm?.get('idDoc_Par_Tipo')?.value); }
  get SolaLettura(): boolean { return !this.Nuovo && !(this.dettaglio?.modificabile ?? false); }
  get PresaVisioneDaFare(): boolean { const r = this.dettaglio?.riepilogo; return !!r && r.presaVisioneRichiesta && !r.dataPresaVisione; }
  // scelta multipla del dipendente: solo per un documento nuovo con tipologia Personale a invio multiplo
  get Multiplo(): boolean { return this.Nuovo && this.Tipo?.categoria === DocCategoria.Personale && this.Tipo?.tipoInvio === DocTipoInvio.Multiplo; }
  // riepilogo nella pagina: il nome del dipendente scelto; con l'invio multiplo e piu' dipendenti solo quanti sono
  get dipendentiLabel(): string {
    if (this.Multiplo && this.dipendentiSelezionati.length > 1)
      return this.transloco.translate('documents.pages.edit.employeesSelected', { n: this.dipendentiSelezionati.length });
    return this.dipendentiSelezionati
      .map(id => this.dipendenti.find(d => d.id === id)?.nominativo)
      .filter(n => !!n).join(', ');
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      idDoc_Par_Tipo: [null, [Validators.required]],
      idDoc_Cartella: [null],
      idDip_Anagrafica: [null],
      idAz_Cliente: [null],
      titolo: [null, [Validators.required, Validators.maxLength(200)]],
      descrizione: [null, [Validators.maxLength(1000)]],
      dataDocumento: [null],
      anno: [null],
      mese: [null, [Validators.min(1), Validators.max(12)]],
      dataScadenza: [null],
      stato: [1],
      noteVersione: [null],
    });
  }

  override ionViewWillEnter() {
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => {
      if (res.success && res.data) {
        this.permessi = res.data.permessi;
        this.tipi = res.data.tipi;
        this.cartelle = res.data.cartelle;
        this.dipendenti = res.data.dipendenti;
      }
      super.ionViewWillEnter();
    });
    this.service.ClienteList().subscribe(res => this.clienti = res.success && res.data ? res.data.az_Cliente : []);
  }

  LoadData = (): Observable<DocDocumentoEditModel | null> => {
    const state = history.state;
    if (state && state.id) {
      const request = new GenericRequest<DocIdInModel>(DocIdInModel);
      request.data.id = state.id;
      return this.service.Get(request).pipe(
        map(res => {
          if (!res.success || !res.data) return null;
          this.mostra(res.data.dettaglio);
          return this._editModel;
        }),
        catchError(() => of(null))
      );
    }
    const nuovo = new DocDocumentoEditModel();
    nuovo.idDoc_Cartella = state?.idDoc_Cartella ?? null;
    nuovo.idDip_Anagrafica = state?.idDip_Anagrafica ?? null;
    this.dipendentiSelezionati = nuovo.idDip_Anagrafica ? [nuovo.idDip_Anagrafica] : [];
    return of(nuovo);
  };

  private mostra(dettaglio: DocDettaglioModel) {
    this.dettaglio = dettaglio;
    const d = { ...dettaglio.documento };
    d.dataDocumento = docDateInput(d.dataDocumento);
    d.dataScadenza = docDateInput(d.dataScadenza);
    this._editModel = d;
    this.dipendentiSelezionati = d.idDip_Anagrafica ? [d.idDip_Anagrafica] : [];
    if (this._editForm) {
      this._editForm.patchValue(d);
      if (!dettaglio.modificabile) this._editForm.disable(); else this._editForm.enable();
      this.btnConfirm.disabled = !dettaglio.modificabile;
    }
  }

  SaveData = (editModel: DocDocumentoEditModel): Observable<boolean> => {
    const dati: DocDocumentoEditModel = {
      ...editModel,
      anno: editModel.anno ? Number(editModel.anno) : null,
      mese: editModel.mese ? Number(editModel.mese) : null,
      dataDocumento: editModel.dataDocumento || null,
      dataScadenza: editModel.dataScadenza || null,
    };
    const personale = this.Tipo?.categoria === DocCategoria.Personale;
    if (this.Nuovo) {
      if (!this.file) {
        this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.edit.fileRequired'));
        return of(false);
      }
      if (personale && this.dipendentiSelezionati.length === 0) {
        this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.edit.employeeRequired'));
        return of(false);
      }
      // invio multiplo: un documento del personale per ogni dipendente scelto (stesso file)
      if (personale && this.Multiplo) {
        const multi = new DocDocumentoUploadMultiploModel();
        multi.documento = { ...dati, idDip_Anagrafica: null };
        multi.idDip_Anagrafica = [...this.dipendentiSelezionati];
        return this.service.UploadMultiplo(multi, this.file).pipe(
          map(res => {
            if (res.success && res.data) { this.multiploCreati = res.data.creati; this.multiploErrori = res.data.errori; }
            return res.success;
          }),
          catchError(() => of(false)));
      }
      // invio singolo
      dati.idDip_Anagrafica = personale ? (this.dipendentiSelezionati[0] ?? null) : dati.idDip_Anagrafica;
      return this.service.Upload(dati, this.file).pipe(map(res => res.success), catchError(() => of(false)));
    }
    const request = new GenericRequest<DocDocumentoPutInModel>(DocDocumentoPutInModel);
    request.data.documento = dati;
    return this.service.Put(request).pipe(
      map(res => {
        if (res.success && res.data) this.mostra(res.data.dettaglio);
        return res.success;
      }),
      catchError(() => of(false))
    );
  };

  override AfterSave(saved: boolean): void {
    if (!saved || !this.Nuovo)
      return;
    // invio multiplo con errori: resto sulla pagina e mostro quanti documenti sono stati creati e cosa non e' riuscito
    if (this.multiploErrori.length) {
      const msg = this.transloco.translate('documents.pages.edit.multiResult', { creati: this.multiploCreati, totale: this.dipendentiSelezionati.length })
                  + '\n' + this.multiploErrori.join('\n');
      this.collectionDialogService.ConfirmCancelDialog(msg);
      this.multiploErrori = [];
      return;
    }
    this.navCtrl.back();
  }

  // scelta del dipendente con il dialog standard: singola o multipla secondo il tipo di invio della tipologia
  async selezionaDipendenti() {
    if (this.SolaLettura)
      return;
    const multiplo = this.Multiplo;
    // elenco per il dialog: seconda riga codice fiscale (o nome utente), piu' la matricola
    const items: DipSelectorItem[] = this.dipendenti.map(d => ({
      id: d.id,
      cognome: d.cognome ?? d.nominativo,
      nome: d.nome ?? '',
      dettaglio: d.codiceFiscale ?? d.userName,
      matricola: d.matricola,
    }));
    const modal = await this.modalCtrl.create({
      component: DipSelectorModalComponent,
      componentProps: { mode: multiplo ? 'multi' : 'single', items, preselected: [...this.dipendentiSelezionati] }
    });
    await modal.present();
    const { data, role } = await modal.onDidDismiss<DipSelectorResult<number>>();
    if (role === 'confirm' && data) {
      this.dipendentiSelezionati = data.selected;
      // per l'invio singolo il campo del documento porta l'unico dipendente scelto
      this._editForm.patchValue({ idDip_Anagrafica: data.selected[0] ?? null });
    }
  }

  // titolo proposto dal nome del file
  onFile(event: Event) {
    const input = event.target as HTMLInputElement;
    this.file = input.files && input.files.length ? input.files[0] : null;
    if (this.file && !this._editForm.get('titolo')?.value)
      this._editForm.patchValue({ titolo: this.file.name.replace(/\.[^.]+$/, '') });
  }

  onNewVersion(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files.length ? input.files[0] : null;
    input.value = '';
    if (!file || !this._editModel) return;
    this.service.NewVersion(this._editModel.id, file).subscribe(res => {
      if (res.success && res.data) this.mostra(res.data.dettaglio);
    });
  }

  download(versione: number | null = null) {
    if (this._editModel) this.service.Download(this._editModel.id, versione);
  }

  presaVisione() {
    if (!this._editModel) return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = this._editModel.id;
    this.service.PresaVisione(request).subscribe(res => {
      if (res.success && this.dettaglio) this.dettaglio.riepilogo.dataPresaVisione = new Date().toISOString();
    });
  }

  async cestino() {
    if (!this._editModel || !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.edit.confirmTrash')))
      return;
    const request = new GenericRequest<DocIdInModel>(DocIdInModel);
    request.data.id = this._editModel.id;
    this.service.Cestino(request).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }
}
