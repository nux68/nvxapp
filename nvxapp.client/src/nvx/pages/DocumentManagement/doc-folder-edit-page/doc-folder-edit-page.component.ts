import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocCartellaModel, DocCartellaPutInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

@Component({
  selector: 'app-doc-folder-edit-page',
  templateUrl: './doc-folder-edit-page.component.html',
  styleUrls: ['./doc-folder-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocFolderEditPageComponent extends BasePageConfirmCancelComponent<DocCartellaModel> {

  public setup: any = history.state?.setup ?? null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.folderEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      idDoc_Cartella: [null],
      ordine: [0],
      visibileUtenti: [false],
    });
  }

  LoadData = (): Observable<DocCartellaModel | null> => of(history.state?.item ?? new DocCartellaModel());

  SaveData = (editModel: DocCartellaModel): Observable<boolean> => {
    const request = new GenericRequest<DocCartellaPutInModel>(DocCartellaPutInModel);
    request.data.cartella = { ...editModel, ordine: Number(editModel.ordine) || 0 };
    return this.service.CartellaPut(request).pipe(
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
}
