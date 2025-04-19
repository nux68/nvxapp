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
import { UserFinancialAdvisorEditModel, UserFinancialAdvisorGetInModel, UserFinancialAdvisorPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-financial-advisor-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';



@Component({
  selector: 'app-user-financial-advisor-edit-page',
  templateUrl: './user-financial-advisor-edit-page.component.html',
  styleUrls: ['./user-financial-advisor-edit-page.component.scss'],
  standalone:false
})
export class UserFinancialAdvisorEditPageComponent extends BasePageConfirmCancelComponent<UserFinancialAdvisorEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "User FinancialAdvisor"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roleId: [null, [Validators.required]],

    });
  }

  LoadData = (): Observable<UserFinancialAdvisorEditModel | null> => {
    const state = history.state;


    if (state && state.id) {
      let request: GenericRequest<UserFinancialAdvisorGetInModel> = new GenericRequest<UserFinancialAdvisorGetInModel>(UserFinancialAdvisorGetInModel);
      request.data.id = state.id;

      return this.accountService.UserFinancialAdvisorGet(request).pipe(
        map((res) => res.data.userFinancialAdvisorEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<UserFinancialAdvisorEditModel | null>((subscriber) => {
        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new UserFinancialAdvisorEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: UserFinancialAdvisorEditModel): Observable<boolean> => {
    let request: GenericRequest<UserFinancialAdvisorPutInModel> =
      new GenericRequest<UserFinancialAdvisorPutInModel>(UserFinancialAdvisorPutInModel);
    request.data.userFinancialAdvisorEdit = editModel;

    return this.accountService.UserFinancialAdvisorPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  getRoler(): RolesModel[] {

    if (this._editModel && this._editModel.idUserFinancialAdvisor == 0) {
      return this.parameterService.Roles.filter(role => (role.code == RoleCode.FinancialAdvisorPowerAdmin || role.code == RoleCode.FinancialAdvisorAdmin));
    }

    return this.parameterService.Roles;
  }




}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
