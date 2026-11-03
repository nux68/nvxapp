import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, forkJoin, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { TicketLevel, TicketTypeOptionModel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import {
  DocumentEditModel, DocumentFileGetInModel, DocumentFileTextInModel, DocumentGetInModel, DocumentPutInModel, DocumentScopeInModel, DocumentSection
} from '../../../ClientServer-Service/Infrastructure/Documentation/Models/document-model';
import { DocumentService } from '../../../ClientServer-Service/Infrastructure/Documentation/document.service';

// stessi limiti del server (DocumentService): il server li ricontrolla comunque
export const DOCUMENT_FILE_MAX_SIZE = 10 * 1024 * 1024;
export const DOCUMENT_CONTENT_MAX_LENGTH = 100000;
export const DOCUMENT_FILE_EXTENSIONS = ['.pdf', '.docx', '.odt', '.ods', '.xlsx', '.xlsm', '.txt', '.md', '.csv', '.htm', '.html', '.json', '.xml'];

/*
 Documento per le risposte automatiche AI: titolo, descrizione (va anche all'AI), tipi di richiesta a cui risponde
 (nessuno = tutti), applicativo, "vale anche per i livelli sotto", contenuto (testo passato all'AI), ordine, attivo.
 Il contenuto si scrive o si estrae dal file caricato (il server legge pdf, Word, OpenDocument, Excel e testo);
 il file resta come originale scaricabile. Gli Admin vedono il documento in sola lettura.
*/
@Component({
  selector: 'app-ai-document-edit-page',
  templateUrl: './ai-document-edit-page.component.html',
  styleUrls: ['./ai-document-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AiDocumentEditPageComponent extends BasePageConfirmCancelComponent<DocumentEditModel> {

  public readonly applications = Object.values(ApplicationType).filter((v): v is ApplicationType => typeof v === 'number' && v !== ApplicationType.Common);
  public readonly accept = DOCUMENT_FILE_EXTENSIONS.join(',');
  public readonly contentMaxLength = DOCUMENT_CONTENT_MAX_LENGTH;
  public ticketTypes: TicketTypeOptionModel[] = [];
  public fileError: string | null = null;
  public reading = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private collectionDialogService: CollectionDialogService,
              private documentService: DocumentService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.aiDocumentEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      titolo: [null, [Validators.required, Validators.maxLength(200)]],
      descrizione: [null, [Validators.maxLength(1000)]],
      idTicketType: [[]],
      applicationType: [null],
      includeSubLevels: [false],
      contenuto: [null, [Validators.required, Validators.maxLength(DOCUMENT_CONTENT_MAX_LENGTH)]],
      ordine: [0],
      active: [true],
    });
  }

  get canEdit(): boolean { return this._editModel?.canEdit ?? false; }

  // livello del documento: per un documento del SuperUser "vale anche per i livelli sotto" vuol dire tutti
  get level(): TicketLevel | null { return this._editModel?.level ?? null; }

  LoadData = (): Observable<DocumentEditModel | null> => {
    const state = history.state;
    const level: TicketLevel | null = state?.level ?? null;

    const scopeRequest = new GenericRequest<DocumentScopeInModel>(DocumentScopeInModel);
    scopeRequest.data.section = DocumentSection.TicketAi;

    let document$: Observable<DocumentEditModel | null>;
    if (state && state.id) {
      const request = new GenericRequest<DocumentGetInModel>(DocumentGetInModel);
      request.data.id = state.id;
      document$ = this.documentService.DocumentGet(request).pipe(map(res => res.data?.document ?? null));
    }
    else {
      const nuovo = new DocumentEditModel();
      nuovo.level = level ?? TicketLevel.SuperUser;
      nuovo.section = DocumentSection.TicketAi;
      document$ = of(nuovo);
    }

    return forkJoin([this.documentService.DocumentScope(scopeRequest), document$]).pipe(
      map(([scopes, document]) => {
        if (!document)
          return null;
        const scope = (scopes.data?.scope ?? []).find(s => s.level === document.level);
        this.ticketTypes = scope?.ticketType ?? [];
        // tipi gia' collegati ma non piu' proponibili (es. disattivati): restano visibili
        for (const id of document.idTicketType.filter(id => !this.ticketTypes.some(t => t.id === id)))
          this.ticketTypes.push(Object.assign(new TicketTypeOptionModel(), { id, descrizione: '#' + id }));
        if (!document.canEdit) {
          this._editForm.disable();
          this.buttonbar = [this.btnCancel];
        }
        return document;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: DocumentEditModel): Observable<boolean> => {
    const request = new GenericRequest<DocumentPutInModel>(DocumentPutInModel);
    request.data.document = { ...editModel, ordine: Number(editModel.ordine) || 0 };

    return this.documentService.DocumentPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }

  // file scelto: il server ne estrae il testo, che sostituisce il contenuto (con conferma se c'era gia' del testo)
  async onFile(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    this.fileError = null;
    if (!file || !this._editModel)
      return;

    const ext = file.name.includes('.') ? file.name.substring(file.name.lastIndexOf('.')).toLowerCase() : '';
    if (!DOCUMENT_FILE_EXTENSIONS.includes(ext)) {
      this.fileError = this.transloco.translate('pages.aiDocumentEdit.fileExtension', { name: file.name });
      return;
    }
    if (file.size === 0 || file.size > DOCUMENT_FILE_MAX_SIZE) {
      this.fileError = this.transloco.translate('pages.aiDocumentEdit.fileSize', { name: file.name, max: DOCUMENT_FILE_MAX_SIZE / (1024 * 1024) });
      return;
    }

    const base64 = await this.toBase64(file);
    const request = new GenericRequest<DocumentFileTextInModel>(DocumentFileTextInModel);
    request.data.fileName = file.name;
    request.data.file = base64;
    this.reading = true;
    this.documentService.DocumentFileText(request).subscribe({
      next: async res => {
        this.reading = false;
        if (!res.success || !res.data || !this._editModel)
          return;

        const current = (this._editForm.get('contenuto')?.value ?? '').trim();
        if (current.length > 0 && !(await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.aiDocumentEdit.replaceContent'))))
          return;

        this._editModel.file = base64;
        this._editModel.fileName = file.name;
        this._editModel.contentType = file.type || 'application/octet-stream';
        this._editModel.size = file.size;
        this._editModel.removeFile = false;
        this._editForm.patchValue({ contenuto: res.data.text });
        if (!(this._editForm.get('titolo')?.value ?? '').trim())
          this._editForm.patchValue({ titolo: file.name.substring(0, file.name.length - ext.length).substring(0, 200) });
        this._editForm.markAsDirty();
      },
      error: () => this.reading = false
    });
  }

  // toglie il file originale (il contenuto resta)
  removeFile() {
    if (!this._editModel)
      return;
    this._editModel.file = null;
    this._editModel.fileName = null;
    this._editModel.contentType = null;
    this._editModel.size = 0;
    this._editModel.removeFile = true;
  }

  // scarica il file salvato (non quello appena scelto e non ancora salvato)
  download() {
    if (!this._editModel?.id || this._editModel.file)
      return;
    const request = new GenericRequest<DocumentFileGetInModel>(DocumentFileGetInModel);
    request.data.id = this._editModel.id;
    this.documentService.DocumentFileGet(request).subscribe(res => {
      if (!res.success || !res.data?.file)
        return;
      const bytes = Uint8Array.from(atob(res.data.file), c => c.charCodeAt(0));
      const url = URL.createObjectURL(new Blob([bytes], { type: res.data.contentType || 'application/octet-stream' }));
      const link = document.createElement('a');
      link.href = url;
      link.download = res.data.fileName;
      link.click();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    });
  }

  get contentLength(): number {
    return (this._editForm.get('contenuto')?.value ?? '').length;
  }

  // contenuto in base64 (senza il prefisso "data:...;base64,")
  private toBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result.substring(result.indexOf(',') + 1));
      };
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }
}
