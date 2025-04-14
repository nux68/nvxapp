import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FinancialAdvisorEditModel, FinancialAdvisorGetInModel, FinancialAdvisorPutInModel } from '../../ClientServer-Service/Account/Models/financial-advisor-model';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../Utility/string-helper.service';



@Component({
  selector: 'app-financial-advisor-edit-page',
  templateUrl: './financial-advisor-edit-page.component.html',
  styleUrls: ['./financial-advisor-edit-page.component.scss'],
  standalone: false
})
export class FinancialAdvisorEditPageComponent extends BasePageConfirmCancelComponent<FinancialAdvisorEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private stringHelperService: StringHelperService,
              private accountService: AccountService)
  {

    super(navCtrl, userInterfaceService, fb);

  }

  
  get Title(): string { return "FinancialAdvisorEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      
      descrizione: [null, [Validators.required, Validators.maxLength(20)]],
      
    });
  }
    
  LoadData = (): Observable<FinancialAdvisorEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<FinancialAdvisorGetInModel> = new GenericRequest<FinancialAdvisorGetInModel>(FinancialAdvisorGetInModel);
      request.data.id = state.id;

      return this.accountService.FinancialAdvisorGet(request).pipe(
        map((res) => res.data.financialAdvisorEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<FinancialAdvisorEditModel | null>((subscriber) => {

        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new FinancialAdvisorEditModel()); 
        subscriber.complete();
      });
    }
  };
  
  SaveData = (editModel: FinancialAdvisorEditModel): Observable<boolean> => {
      let request: GenericRequest<FinancialAdvisorPutInModel> = 
        new GenericRequest<FinancialAdvisorPutInModel>(FinancialAdvisorPutInModel);
      request.data.financialAdvisorEdit = editModel;

      return this.accountService.FinancialAdvisorPut(request).pipe(
        map(() => true), // Restituisce true in caso di successo
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [false]; // Restituisce false in caso di errore
        })
      );
    };

  UpdateDescription() {
    const descrizione = this._editForm.get('descrizione')?.value;

    if (descrizione) {
      this.modifiedDescription = "Attenzione per accedere a questa utenza verrano creati i seguenti user    ->   " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_Admin" + " / " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_PowerAdmin";
    } else {
      this.modifiedDescription = null;
    }
  }
  

}

const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
