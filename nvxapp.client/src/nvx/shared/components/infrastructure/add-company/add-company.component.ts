import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { CompanyEditModel, CompanyPutInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/company-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';

@Component({
  selector: 'app-add-company',
  templateUrl: './add-company.component.html',
  styleUrls: ['./add-company.component.scss'],
  standalone: false
})
export class AddCompanyComponent  extends BaseDialogConfirmCancelComponent<CompanyEditModel> {

  @Input() companyEdit: CompanyEditModel;

  modifiedDescription: string | null = null;

  constructor(protected override userInterfaceService: UserInterfaceService,
      protected override fb: FormBuilder,
      protected override modalCtrl: ModalController,
       private accountService: AccountService
      ) 
  {
    super(userInterfaceService, fb, modalCtrl);

  }

  override ngOnInit() {
    super.ngOnInit();
    // Sovrascrive l'handler del bottone conferma per eseguire il salvataggio via API
    this.buttonbar[0].event = () => this.handleConfirm();
  }

 get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(20)]],

    });
  }


  get Title(): string { return "Inserimento nuova azienda"; }

  LoadData = (): Observable<CompanyEditModel | null> => {
    // Aggiunge campi per la nuova registrazione (mail, pw, confirmPassword)
    this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
    this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
    this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
    this._editForm.setValidators(matchPasswords);
    this._editForm.updateValueAndValidity();

    return of(this.companyEdit);
  }

  SaveData = (editModel: CompanyEditModel): Observable<CompanyEditModel> => {
    let request: GenericRequest<CompanyPutInModel> = new GenericRequest<CompanyPutInModel>(CompanyPutInModel);
    request.data.companyEdit = editModel;

    return this.accountService.CompanyPut(request).pipe(
      map((res) => res.data.companyEdit),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  }

  private handleConfirm() {
    // Forza la validazione su tutto il form
    Object.keys(this._editForm.controls).forEach((key) => {
      const control = this._editForm.get(key);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });

    if (this._editForm.valid) {
      Object.assign(this._editModel, this._editForm.value);
      this.SaveData(this._editModel).subscribe(res => {
        if (res) {
          this.modalCtrl.dismiss(res, 'confirm');
        }
      });
    }
  }
}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
