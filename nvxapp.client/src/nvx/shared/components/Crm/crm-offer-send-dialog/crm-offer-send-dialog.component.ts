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
import { CrmOffertaInviaInModel, CrmOffertaModel } from '../../../../ClientServer-Service/Crm/Models/crm-model';

// Invio dell'offerta per email (PDF allegato); l'offerta diventa "Inviata"
@Component({
  selector: 'crm-offer-send-dialog',
  templateUrl: './crm-offer-send-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CrmOfferSendDialogComponent extends BaseDialogConfirmCancelComponent<CrmOffertaInviaInModel> {

  @Input() idOfferta = 0;
  @Input() email = '';

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

  get Title(): string { return this.transloco.translate('crm.send.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      email: [null, [Validators.required, Validators.email, Validators.maxLength(200)]],
      messaggio: [null, [Validators.maxLength(4000)]],
    });
  }

  LoadData = (): Observable<CrmOffertaInviaInModel | null> => of(Object.assign(new CrmOffertaInviaInModel(), { id: this.idOfferta, email: this.email }));

  SaveData = (editModel: CrmOffertaInviaInModel): Observable<any> => {
    const request = new GenericRequest<CrmOffertaInviaInModel>(CrmOffertaInviaInModel);
    request.data = { ...editModel, id: this.idOfferta };
    return this.service.OffertaInvia(request).pipe(map(res => res.success ? res.data.offerta : null), catchError(() => of(null)));
  };

  valido(): boolean { return true; }
}
