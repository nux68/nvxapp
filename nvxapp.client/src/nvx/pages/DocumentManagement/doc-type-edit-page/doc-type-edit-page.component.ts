import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocTipoModel, DocTipoPutInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

@Component({
  selector: 'app-doc-type-edit-page',
  templateUrl: './doc-type-edit-page.component.html',
  styleUrls: ['./doc-type-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocTypeEditPageComponent extends BasePageConfirmCancelComponent<DocTipoModel> {

  public setup: any = history.state?.setup ?? null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.typeEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(20)]],
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      categoria: [1, [Validators.required]],
      anniConservazione: [null, [Validators.min(1)]],
      haScadenza: [false],
      presaVisioneRichiesta: [false],
      haCompetenza: [false],
      ordine: [0],
      active: [true],
    });
  }

  LoadData = (): Observable<DocTipoModel | null> => of(history.state?.item ?? new DocTipoModel());

  SaveData = (editModel: DocTipoModel): Observable<boolean> => {
    const request = new GenericRequest<DocTipoPutInModel>(DocTipoPutInModel);
    request.data.tipo = { ...editModel, anniConservazione: editModel.anniConservazione ? Number(editModel.anniConservazione) : null, ordine: Number(editModel.ordine) || 0 };
    return this.service.TipoPut(request).pipe(
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
