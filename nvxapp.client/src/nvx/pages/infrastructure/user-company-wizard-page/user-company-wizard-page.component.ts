import { Component, OnInit } from '@angular/core';

import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError, of } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RolesModel } from '../../../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';

import { UserCompanyEditModel, UserCompanyGetInModel, UserCompanyPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-company-wizard-page',
  templateUrl: './user-company-wizard-page.component.html',
  styleUrls: ['./user-company-wizard-page.component.scss'],
  standalone: false
})
export class UserCompanyWizardPageComponent  extends BasePageConfirmCancelComponent<UserCompanyEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }

  get Title(): string { return "User Company WizardPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      //roleId: [null, [Validators.required]],
      roles: [[], this.minArrayLength(1)],

    });
  }

  minArrayLength(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      return control.value && control.value.length >= min ? null : { minArrayLength: true };
    };
  }

  LoadData = (): Observable<UserCompanyEditModel | null> => {
    const state = history.state;


    if (state && state.id) {
      let request: GenericRequest<UserCompanyGetInModel> = new GenericRequest<UserCompanyGetInModel>(UserCompanyGetInModel);
      request.data.id = state.id;

      return this.accountService.UserCompanyGet(request).pipe(
        map((res) => res.data.userCompanyEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null); // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<UserCompanyEditModel | null>((subscriber) => {
        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new UserCompanyEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: UserCompanyEditModel): Observable<boolean> => {
    let request: GenericRequest<UserCompanyPutInModel> =
      new GenericRequest<UserCompanyPutInModel>(UserCompanyPutInModel);
    request.data.userCompanyEdit = editModel;

    return this.accountService.UserCompanyPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  getRoler(): RolesModel[] {

    if (this._editModel && this._editModel.idUserCompany == 0) {
      return this.parameterService.Roles.filter(role => role.code == RoleCode.User);
    }

    return this.parameterService.Roles;
  }


  getRoles(): string[] {

    if (this._editModel) {
      const rolesPowerAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyPowerAdmin);

      if (this._editModel.roles.includes(rolesPowerAdmin.name)) {
        return this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyPowerAdmin)
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

}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};