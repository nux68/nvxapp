import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { UserCompanyEditModel } from 'src/nvx/ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { RoleCode } from 'src/nvx/ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ParameterService } from 'src/nvx/ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { BasePageConfirmCancelComponent } from 'src/nvx/pages/_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from 'src/nvx/Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-add-user-company',
  templateUrl: './add-user-company.component.html',
  styleUrls: ['./add-user-company.component.scss'],
  standalone: false
})
export class AddUserCompanyComponent  extends BasePageConfirmCancelComponent<UserCompanyEditModel> {

  @Input() userCompanyEdit: UserCompanyEditModel;

  modifiedDescription: string | null = null;

  constructor( protected override navCtrl: NavController,
      protected override userInterfaceService: UserInterfaceService,
      protected override fb: FormBuilder,
       private parameterService: ParameterService
      ) 
  {
    super(navCtrl, userInterfaceService, fb);

  }

 get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roles: [[], this.minArrayLength(1)],

    });
  }


  get Title(): string { return "AddUserCompany"; }

  LoadData = (): Observable<UserCompanyEditModel | null> => {
    // Aggiunge campi per la nuova registrazione (mail, pw, confirmPassword)
    this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
    this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
    this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
    this._editForm.setValidators(matchPasswords);
    this._editForm.updateValueAndValidity();

    return of(this.userCompanyEdit);
  }

  SaveData = (editModel: UserCompanyEditModel): Observable<boolean> => {
    return of(true);
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
