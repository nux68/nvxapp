import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BaseDialogConfirmCancelComponent } from '../../../_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { DocumentManagementService } from '../../../../ClientServer-Service/DocumentManagement/document-management.service';
import {
  DocCategoria, DocDipendenteModel, DocDocumentoEditModel, DocDocumentoUploadMultiploModel, DocDocumentoUploadMultiploOutModel, DocStato, DocTipoModel
} from '../../../../ClientServer-Service/DocumentManagement/Models/doc-model';

/*
 Invio di un documento a piu' dipendenti: si scelgono i dipendenti (ricerca, "seleziona tutti", solo con account),
 la tipologia del personale, i dati e il file. Il server crea un documento del personale per ciascun dipendente
 (con il suo destinatario e la presa visione della tipologia) e restituisce quanti ne ha creati e gli eventuali errori.
*/
@Component({
  selector: 'app-doc-multi-send-dialog',
  templateUrl: './doc-multi-send-dialog.component.html',
  styleUrls: ['./doc-multi-send-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocMultiSendDialogComponent extends BaseDialogConfirmCancelComponent<DocDocumentoEditModel> {

  @Input() dipendenti: DocDipendenteModel[] = [];
  @Input() tipi: DocTipoModel[] = [];

  public selezionati = new Set<number>();
  public ricerca = '';
  public soloConAccount = false;
  public file: File | null = null;
  public esito: DocDocumentoUploadMultiploOutModel | null = null;
  public invioInCorso = false;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private service: DocumentManagementService,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ngOnInit() {
    super.ngOnInit();
    this.buttonbar[0].text = this.transloco.translate('documents.multiSend.send');
    this.buttonbar[0].event = () => this.invia();
  }

  get Title(): string { return this.transloco.translate('documents.multiSend.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idDoc_Par_Tipo: [null, [Validators.required]],
      titolo: [null, [Validators.required, Validators.maxLength(200)]],
      descrizione: [null],
      dataDocumento: [null],
      mese: [null, [Validators.min(1), Validators.max(12)]],
      anno: [null, [Validators.min(1900), Validators.max(2200)]],
      dataScadenza: [null],
      stato: [DocStato.Pubblicato],
    });
  }

  LoadData = (): Observable<DocDocumentoEditModel | null> => of(new DocDocumentoEditModel());

  SaveData = (editModel: DocDocumentoEditModel): Observable<any> => {
    const data = new DocDocumentoUploadMultiploModel();
    data.documento = { ...editModel, mese: editModel.mese ? Number(editModel.mese) : null, anno: editModel.anno ? Number(editModel.anno) : null,
                       dataDocumento: editModel.dataDocumento || null, dataScadenza: editModel.dataScadenza || null };
    data.idDip_Anagrafica = [...this.selezionati];
    return this.service.UploadMultiplo(data, this.file!).pipe(map(res => res.success ? res.data : null), catchError(() => of(null)));
  };

  // solo tipologie del personale attive
  get tipiPersonale(): DocTipoModel[] {
    return this.tipi.filter(t => t.categoria === DocCategoria.Personale && t.active);
  }

  get Tipo(): DocTipoModel | undefined {
    return this.tipi.find(t => t.id === this._editForm?.get('idDoc_Par_Tipo')?.value);
  }

  // dipendenti attivi, filtrati per ricerca e account
  get elenco(): DocDipendenteModel[] {
    const r = this.ricerca.trim().toLowerCase();
    return this.dipendenti.filter(d => d.attivo && (!this.soloConAccount || d.haUtente) &&
      (!r || [d.nominativo, d.codiceFiscale, d.matricola, d.userName].some(v => (v ?? '').toLowerCase().includes(r))));
  }

  get tuttiSelezionati(): boolean {
    const e = this.elenco;
    return e.length > 0 && e.every(d => this.selezionati.has(d.id));
  }

  selezionaTutti(valore: boolean) {
    for (const d of this.elenco) {
      if (valore) this.selezionati.add(d.id); else this.selezionati.delete(d.id);
    }
  }

  seleziona(d: DocDipendenteModel, valore: boolean) {
    if (valore) this.selezionati.add(d.id); else this.selezionati.delete(d.id);
  }

  scegliFile(ev: Event) {
    const input = ev.target as HTMLInputElement;
    this.file = input.files?.[0] ?? null;
  }

  private invia() {
    // a invio fatto il pulsante chiude il dialog
    if (this.esito) {
      this.modalCtrl.dismiss(this.esito, 'confirm');
      return;
    }
    Object.values(this._editForm.controls).forEach(c => { c.markAsTouched(); c.updateValueAndValidity(); });
    if (!this._editForm.valid || this.selezionati.size === 0 || !this.file || this.invioInCorso)
      return;
    Object.assign(this._editModel, this._editForm.value);
    this.invioInCorso = true;
    this.SaveData(this._editModel).subscribe(res => {
      this.invioInCorso = false;
      if (!res)
        return;
      this.esito = res;
      this.buttonbar[0].text = this.transloco.translate('documents.multiSend.close');
      if (res.errori.length === 0)
        this.modalCtrl.dismiss(res, 'confirm');
    });
  }
}
