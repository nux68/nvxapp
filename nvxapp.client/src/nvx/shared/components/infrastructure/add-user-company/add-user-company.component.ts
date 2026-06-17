import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { UserCompanyEditModel, UserCompanyPutInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';

@Component({
  selector: 'app-add-user-company',
  templateUrl: './add-user-company.component.html',
  styleUrls: ['./add-user-company.component.scss'],
  standalone: false
})
export class AddUserCompanyComponent  extends BaseDialogConfirmCancelComponent<UserCompanyEditModel> {

  @Input() userCompanyEdit: UserCompanyEditModel;

  modifiedDescription: string | null = null;

  constructor(protected override userInterfaceService: UserInterfaceService,
      protected override fb: FormBuilder,
      protected override modalCtrl: ModalController,
       private parameterService: ParameterService,
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

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roles: [[], this.minArrayLength(1)],

    });
  }


  get Title(): string { return "Inserimento nuovo utente"; }

  LoadData = (): Observable<UserCompanyEditModel | null> => {
    // Aggiunge campi per la nuova registrazione (mail, pw, confirmPassword)
    this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
    this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
    this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
    this._editForm.setValidators(matchPasswords);
    this._editForm.updateValueAndValidity();

    // Imposta il ruolo "User" come default se non già valorizzato
    if (!this.userCompanyEdit.roles || this.userCompanyEdit.roles.length === 0) {
      const userRole = this.parameterService.Roles.find(role => role.code === RoleCode.User);
      if (userRole) {
        this.userCompanyEdit.roles = [userRole.name];
      }
    }

    return of(this.userCompanyEdit);
  }

  SaveData = (editModel: UserCompanyEditModel): Observable<UserCompanyEditModel> => {
    let request: GenericRequest<UserCompanyPutInModel> = new GenericRequest<UserCompanyPutInModel>(UserCompanyPutInModel);
    request.data.userCompanyEdit = editModel;

    return this.accountService.UserCompanyPut(request).pipe(
      map((res) => res.data.userCompanyEdit),
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

  getRoles(): string[] {

    if (this._editModel) {
      const rolesPowerAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyPowerAdmin);

      if (this._editModel.roles.includes(rolesPowerAdmin.name)) {
        return this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyPowerAdmin )
          .map(role => role.name); 
      }
      else {
        return this.parameterService.Roles.filter(role => role.code == RoleCode.User ||
          role.code == RoleCode.CompanyAdmin)
          .map(role => role.name); 
      }
    }

    return [];
         

  }

  minArrayLength(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      return control.value && control.value.length >= min ? null : { minArrayLength: true };
    };
  }

}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
