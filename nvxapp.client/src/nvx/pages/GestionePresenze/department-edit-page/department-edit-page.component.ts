import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, catchError } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoGetInModel, Az_SediRepartoModel, Az_SediRepartoPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { DealerGetInModel, DealerPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/dealer-model';

@Component({
  selector: 'app-department-edit-page',
  templateUrl: './department-edit-page.component.html',
  styleUrls: ['./department-edit-page.component.scss'],
  standalone: false
}) 
export class DepartmentEditPageComponent extends BasePageConfirmCancelComponent<Az_SediRepartoModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private azSediRepartoService: AzSediRepartoService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "Dealer"; }
  get EditForm(): FormGroup {
    return this.
      fb.group({

        descrizione: [null, [Validators.required, Validators.maxLength(20)]],

      });
  }

  LoadData = (): Observable<Az_SediRepartoModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<Az_SediRepartoGetInModel> = new GenericRequest<Az_SediRepartoGetInModel>(Az_SediRepartoGetInModel);
      request.data.id = state.id;

      return this.azSediRepartoService.Az_SediRepartoGet(request).pipe(
        map((res) => res.data.az_SediReparto), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<Az_SediRepartoModel | null>((subscriber) => {

        //aggiunge campi solo per le new
        //this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        //this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        //this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        //this._editForm.setValidators(matchPasswords);
        //this._editForm.updateValueAndValidity();

        subscriber.next(new Az_SediRepartoModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_SediRepartoModel): Observable<boolean> => {
    let request: GenericRequest<Az_SediRepartoPutInModel> = new GenericRequest<Az_SediRepartoPutInModel>(Az_SediRepartoPutInModel);
    request.data.az_SediReparto = editModel;

    return this.azSediRepartoService.Az_SediRepartoPut(request).pipe(
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
