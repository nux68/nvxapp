import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../Utility/string-helper.service';
import { ParameterService } from '../../ClientServer-Service/Parameter/parameter.service';
import { RolesModel } from '../../ClientServer-Service/Parameter/Models/roles-model';

import { UserCompanyEditModel, UserCompanyGetInModel, UserCompanyPutInModel } from '../../ClientServer-Service/Account/Models/user-company-model';
import { UserDealerGetInModel, UserDealerEditModel, UserDealerPutInModel } from '../../ClientServer-Service/Account/Models/user-dealer-model';

@Component({
  selector: 'app-user-dealer-edit-page',
  templateUrl: './user-dealer-edit-page.component.html',
  styleUrls: ['./user-dealer-edit-page.component.scss'],
  standalone:false
})//UserDealerEditPageComponent
export class UserDealerEditPageComponent extends BasePageConfirmCancelComponent<UserDealerEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "UserDealerEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roleId: [null, [Validators.required]],

    });
  }

  LoadData = (): Observable<UserDealerEditModel | null> => {
    const state = history.state;


    if (state && state.id) {
      let request: GenericRequest<UserDealerGetInModel> = new GenericRequest<UserDealerGetInModel>(UserDealerGetInModel);
      request.data.id = state.id;

      return this.accountService.UserDealerGet(request).pipe(
        map((res) => res.data.userDealerEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<UserDealerEditModel | null>((subscriber) => {
        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new UserDealerEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: UserDealerEditModel): Observable<boolean> => {
    let request: GenericRequest<UserDealerPutInModel> =
      new GenericRequest<UserDealerPutInModel>(UserDealerPutInModel);
    request.data.userDealerEdit = editModel;

    return this.accountService.UserDealerPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  getRoler(): RolesModel[] {

    if (this._editModel && this._editModel.idUserDealer == 0) {
      return this.parameterService.Roles.filter(role => (role.code == 1000 || role.code == 1001));
    }

    return this.parameterService.Roles;
  }




}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};

