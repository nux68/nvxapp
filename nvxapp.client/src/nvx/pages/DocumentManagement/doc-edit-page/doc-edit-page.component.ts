import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DocCartellaModel, DocCategoria, DocClienteModel, DocDettaglioModel, DocDipendenteModel, DocDocumentoEditModel, DocDocumentoPutInModel, DocEmptyInModel, DocIdInModel, DocPermessiModel, DocTipoModel, docDateInput, docSize } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

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

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.edit.title'); }
  get Nuovo(): boolean { return !this._editModel?.id; }
  get Tipo(): DocTipoModel | undefined { return this.tipi.find(t => t.id === this._editForm?.get('idDoc_Par_Tipo')?.value); }
  get SolaLettura(): boolean { return !this.Nuovo && !(this.dettaglio?.modificabile ?? false); }
  get PresaVisioneDaFare(): boolean { const r = this.dettaglio?.riepilogo; return !!r && r.presaVisioneRichiesta && !r.dataPresaVisione; }

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
    return of(nuovo);
  };

  private mostra(dettaglio: DocDettaglioModel) {
    this.dettaglio = dettaglio;
    const d = { ...dettaglio.documento };
    d.dataDocumento = docDateInput(d.dataDocumento);
    d.dataScadenza = docDateInput(d.dataScadenza);
    this._editModel = d;
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
    if (this.Nuovo) {
      if (!this.file) {
        this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('documents.pages.edit.fileRequired'));
        return of(false);
      }
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
    if (saved && this.Nuovo)
      this.navCtrl.back();
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
