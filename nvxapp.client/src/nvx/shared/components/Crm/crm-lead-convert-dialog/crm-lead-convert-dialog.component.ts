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
import { CrmLeadConvertiInModel, CrmLeadConvertiOutModel, CrmClienteListModel } from '../../../../ClientServer-Service/Crm/Models/crm-model';

// Conversione di un lead: cliente nuovo (dai dati del lead) o esistente, con l'eventuale opportunita'
@Component({
  selector: 'crm-lead-convert-dialog',
  templateUrl: './crm-lead-convert-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CrmLeadConvertDialogComponent extends BaseDialogConfirmCancelComponent<CrmLeadConvertiInModel> {

  @Input() idLead = 0;
  @Input() titolo = '';
  @Input() clienti: CrmClienteListModel[] = [];

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

  get Title(): string { return this.transloco.translate('crm.convert.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idAz_ClienteEsistente: [null],
      creaOpportunita: [true],
      titoloOpportunita: [null, [Validators.maxLength(200)]],
      valoreOpportunita: [0, [Validators.min(0)]],
    });
  }

  LoadData = (): Observable<CrmLeadConvertiInModel | null> =>
    of(Object.assign(new CrmLeadConvertiInModel(), { id: this.idLead, titoloOpportunita: this.titolo }));

  SaveData = (editModel: CrmLeadConvertiInModel): Observable<any> => {
    const request = new GenericRequest<CrmLeadConvertiInModel>(CrmLeadConvertiInModel);
    request.data = { ...editModel, id: this.idLead, valoreOpportunita: Number(editModel.valoreOpportunita) || 0 };
    return this.service.LeadConverti(request).pipe(map(res => res.success ? res.data : null), catchError(() => of(null)));
  };

  valido(): boolean { return true; }
}
