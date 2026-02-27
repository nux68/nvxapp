import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_Orario_GetInModel, Par_Orario_PutInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_OrarioIntervalloHHModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';


@Component({
  selector: 'app-orari-edit-page',
  templateUrl: './orari-edit-page.component.html',
  styleUrls: ['./orari-edit-page.component.scss'],
  standalone: false
})
export class OrariEditPageComponent extends BasePageConfirmCancelComponent<Par_OrarioModel> implements OnInit {

  public par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parOrarioService: ParOrarioService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string {
    return "Orario";
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  LoadData = (): Observable<Par_OrarioModel | null> => {
    const state = history.state;

    if (state && state.id ) {
      let request = new GenericRequest<Par_Orario_GetInModel>(Par_Orario_GetInModel);
      request.data.id = state.id;

      return this.parOrarioService.Par_OrarioGet(request).pipe(
        map((res) => {
            this.par_OrarioIntervalloHH = res.data.par_OrarioIntervalloHH;
            return res.data.par_Orario;
          }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Par_OrarioModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();

        subscriber.next(new Par_OrarioModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_OrarioModel): Observable<boolean> => {
    //let request = new GenericRequest<Par_Orario_PutInModel>(Par_Orario_PutInModel)


    let request: GenericRequest<Par_Orario_PutInModel> =
      new GenericRequest<Par_Orario_PutInModel>(Par_Orario_PutInModel);
    request.data.par_Orario = editModel;
    request.data.par_OrarioIntervalloHH = this.par_OrarioIntervalloHH;

    return this.parOrarioService.Par_OrarioPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(); 
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante il salvataggio:', error);
        return [false];
      })
    );
  };
}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};
