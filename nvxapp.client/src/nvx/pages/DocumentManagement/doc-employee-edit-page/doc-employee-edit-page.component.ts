import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocDipendenteModel, DocDipendentePutInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

@Component({
  selector: 'app-doc-employee-edit-page',
  templateUrl: './doc-employee-edit-page.component.html',
  styleUrls: ['./doc-employee-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocEmployeeEditPageComponent extends BasePageConfirmCancelComponent<DocDipendenteModel> {

  public setup: any = history.state?.setup ?? null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.employeeEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      cognome: [null, [Validators.maxLength(100)]],
      nome: [null, [Validators.maxLength(100)]],
      codiceFiscale: [null, [Validators.maxLength(16)]],
      matricola: [null, [Validators.maxLength(20)]],
      email: [null, [Validators.email, Validators.maxLength(100)]],
      dataCessazione: [null],
      attivo: [true],
    });
  }

  LoadData = (): Observable<DocDipendenteModel | null> => of(history.state?.item ?? new DocDipendenteModel());

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    const item = history.state?.item;
    if (item?.dataCessazione) setTimeout(() => this._editForm.patchValue({ dataCessazione: String(item.dataCessazione).substring(0, 10) }));
  }

  SaveData = (editModel: DocDipendenteModel): Observable<boolean> => {
    const request = new GenericRequest<DocDipendentePutInModel>(DocDipendentePutInModel);
    request.data.dipendente = { ...editModel, dataCessazione: editModel.dataCessazione || null };
    return this.service.DipendentePut(request).pipe(
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
