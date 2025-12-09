import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RolesModel } from '../../../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';


import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { Dip_Anagrafica4EditModel, Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Put_InModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DipAnagraficaService } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';

@Component({
  selector: 'app-user-department-edit-page',
  templateUrl: './user-department-edit-page.component.html',
  styleUrls: ['./user-department-edit-page.component.scss'],
  standalone: false
})
export class UserDepartmentEditPageComponent extends BasePageConfirmCancelComponent<Dip_Anagrafica4EditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    //private accountService: AccountService,
    private dipAnagraficaService: DipAnagraficaService) {

    super(navCtrl, userInterfaceService, fb);

  }

  get Title(): string { return "User Department Edit"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roles: [[], this.minArrayLength(1)],
      cognome: [null, [Validators.required, Validators.maxLength(50)]],
      nome: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  minArrayLength(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      return control.value && control.value.length >= min ? null : { minArrayLength: true };
    };
  }

  LoadData = (): Observable<Dip_Anagrafica4EditModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Dip_Anagrafica_Get_InModel> = new GenericRequest<Dip_Anagrafica_Get_InModel>(Dip_Anagrafica_Get_InModel);
      request.data.id = state.id;
      return this.dipAnagraficaService.Dip_AnagraficaGet(request).pipe(
        map((res) => res.data.dip_Anagrafica),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Dip_Anagrafica4EditModel | null>((subscriber) => {
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();
        subscriber.next(new Dip_Anagrafica4EditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Dip_Anagrafica4EditModel): Observable<boolean> => {
    let request: GenericRequest<Dip_Anagrafica_Put_InModel> =
      new GenericRequest<Dip_Anagrafica_Put_InModel>(Dip_Anagrafica_Put_InModel);
    request.data.dip_Anagrafica = editModel;
    request.data.id = editModel.idAspNetUsers;
    return this.dipAnagraficaService.Dip_AnagraficaPut(request).pipe(
      map(() => true),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
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
        return this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyPowerAdmin )
          .map(role => role.name); 
      } else {
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
