import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { CompanyEditModel, CompanyGetInModel, CompanyPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/company-model';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';

@Component({
  selector: 'app-company-edit-page',
  templateUrl: './company-edit-page.component.html',
  styleUrls: ['./company-edit-page.component.scss'],
  standalone:false
})  
export class CompanyEditPageComponent extends BasePageConfirmCancelComponent<CompanyEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private authService: AuthService,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "CompanyEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(20)]],

    });
  }

  LoadData = (): Observable<CompanyEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<CompanyGetInModel> = new GenericRequest<CompanyGetInModel>(CompanyGetInModel);
      request.data.id = state.id;

      return this.accountService.CompanyGet(request).pipe(
        map((res) => res.data.companyEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<CompanyEditModel | null>((subscriber) => {

        //aggiunge campi solo per le new
        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        subscriber.next(new CompanyEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: CompanyEditModel): Observable<boolean> => {
    let request: GenericRequest<CompanyPutInModel> =
      new GenericRequest<CompanyPutInModel>(CompanyPutInModel);
    request.data.companyEdit = editModel;

    return this.accountService.CompanyPut(request).pipe(
      map(() => {
        this.authService.forceRolesEmission(); // Forza l'emissione dei ruoli per aggiornare i parametri
        return true;
      }), // Restituisce true in caso di successo
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

