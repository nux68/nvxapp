import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../../ClientServer-Service/Crm/crm.service';
import { CrmContattoModel, CrmContattoPutInModel } from '../../../../ClientServer-Service/Crm/Models/crm-model';

// Persona di riferimento di un cliente, salvata con le API del CRM (regole di visibilita' del commerciale)
@Component({
  selector: 'crm-contact-dialog',
  templateUrl: './crm-contact-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CrmContactDialogComponent extends BaseDialogConfirmCancelComponent<CrmContattoModel> {

  @Input() contatto: CrmContattoModel = new CrmContattoModel();

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private service: CrmService,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ngOnInit() {
    super.ngOnInit();
    this.buttonbar[0].event = () => this.conferma();
  }

  private conferma() {
    Object.values(this._editForm.controls).forEach(c => { c.markAsTouched(); c.updateValueAndValidity(); });
    if (!this._editForm.valid || !this.valido())
      return;
    Object.assign(this._editModel, this._editForm.value);
    this.SaveData(this._editModel).subscribe(res => {
      if (res)
        this.modalCtrl.dismiss(res, 'confirm');
    });
  }

  get Title(): string { return this.transloco.translate('crm.contact.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      cognome: [null, [Validators.maxLength(100)]],
      nome: [null, [Validators.maxLength(100)]],
      ruolo: [null, [Validators.maxLength(100)]],
      telefono: [null, [Validators.maxLength(30)]],
      cellulare: [null, [Validators.maxLength(30)]],
      email: [null, [Validators.email, Validators.maxLength(100)]],
      principale: [false],
      note: [null, [Validators.maxLength(500)]],
    });
  }

  LoadData = (): Observable<CrmContattoModel | null> => of(this.contatto);

  SaveData = (editModel: CrmContattoModel): Observable<CrmContattoModel | null> => {
    const request = new GenericRequest<CrmContattoPutInModel>(CrmContattoPutInModel);
    request.data.contatto = editModel;
    return this.service.ContattoPut(request).pipe(map(res => res.success ? res.data.contatto : null), catchError(() => of(null)));
  };

  valido(): boolean {
    const v = this._editForm?.value;
    return !!v && !!(v.cognome?.trim() || v.nome?.trim());
  }
}
