import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BaseDialogConfirmCancelComponent } from '../../../_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { Az_ClienteContattoModel, Az_ClienteContattoPutInModel } from '../../../../ClientServer-Service/Infrastructure/Az_ClienteContatto/Models/az-cliente-contatto-model';
import { AzClienteContattoService } from '../../../../ClientServer-Service/Infrastructure/Az_ClienteContatto/az-cliente-contatto.service';

// Persona di riferimento di un cliente: nuova o in modifica, salvata alla conferma
@Component({
  selector: 'app-customer-contact-dialog',
  templateUrl: './customer-contact-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CustomerContactDialogComponent extends BaseDialogConfirmCancelComponent<Az_ClienteContattoModel> {

  @Input() contatto: Az_ClienteContattoModel = new Az_ClienteContattoModel();

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private azClienteContattoService: AzClienteContattoService,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ngOnInit() {
    super.ngOnInit();
    this.buttonbar[0].event = () => this.conferma();
  }

  get Title(): string { return this.transloco.translate('components.customerContact.title'); }

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

  LoadData = (): Observable<Az_ClienteContattoModel | null> => of(this.contatto);

  SaveData = (editModel: Az_ClienteContattoModel): Observable<Az_ClienteContattoModel | null> => {
    const request = new GenericRequest<Az_ClienteContattoPutInModel>(Az_ClienteContattoPutInModel);
    request.data.az_ClienteContatto = editModel;
    return this.azClienteContattoService.Put(request).pipe(
      map(res => res.success ? res.data.az_ClienteContatto : null),
      catchError(() => of(null))
    );
  };

  // cognome o nome obbligatorio (lo ricontrolla il server)
  get nomeMancante(): boolean {
    const v = this._editForm?.value;
    return !!v && !(v.cognome?.trim() || v.nome?.trim());
  }

  private conferma() {
    Object.values(this._editForm.controls).forEach(c => { c.markAsTouched(); c.updateValueAndValidity(); });
    if (!this._editForm.valid || this.nomeMancante)
      return;
    Object.assign(this._editModel, this._editForm.value);
    this.SaveData(this._editModel).subscribe(res => {
      if (res)
        this.modalCtrl.dismiss(res, 'confirm');
    });
  }
}
