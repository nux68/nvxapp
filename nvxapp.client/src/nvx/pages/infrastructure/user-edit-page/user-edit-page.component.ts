import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Parameter/parameter.service';
import { RolesModel } from '../../../ClientServer-Service/Parameter/Models/roles-model';


import { UserEditModel, UserGetInModel, UserPutInModel } from '../../../ClientServer-Service/Account/Models/user-model';
import { RoleCode } from '../../../ClientServer-Service/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-edit-page',
  templateUrl: './user-edit-page.component.html',
  styleUrls: ['./user-edit-page.component.scss'],
  standalone:false
})
export class UserEditPageComponent extends BasePageConfirmCancelComponent<UserEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "UserEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roleId: [null, [Validators.required]],

    });
  }

  LoadData = (): Observable<UserEditModel | null> => {
    const state = history.state;


    if (state && state.id) {
      let request: GenericRequest<UserGetInModel> = new GenericRequest<UserGetInModel>(UserGetInModel);
      request.data.id = state.id;

      return this.accountService.UserGet(request).pipe(
        map((res) => res.data.userEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<UserEditModel | null>((subscriber) => {
        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new UserEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: UserEditModel): Observable<boolean> => {
    let request: GenericRequest<UserPutInModel> =
      new GenericRequest<UserPutInModel>(UserPutInModel);
    request.data.userEdit = editModel;

    return this.accountService.UserPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  getRoler(): RolesModel[] {

    if (this._editModel && this._editModel.descrizione == '') {
      return this.parameterService.Roles.filter(role => role.code == RoleCode.Admin || role.code == RoleCode.PowerAdmin);
    }

    return this.parameterService.Roles;
  }




}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
