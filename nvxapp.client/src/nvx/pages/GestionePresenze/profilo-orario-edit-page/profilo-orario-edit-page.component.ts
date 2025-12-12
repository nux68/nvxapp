import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_ProfiloOrarioModel, Par_ProfiloOrario_GetInModel, Par_ProfiloOrario_PutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { ParProfiloOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-profilo-orario-edit-page',
  templateUrl: './profilo-orario-edit-page.component.html',
  styleUrls: ['./profilo-orario-edit-page.component.scss'],
  standalone: false
})
export class ProfiloOrarioEditPageComponent extends BasePageConfirmCancelComponent<Par_ProfiloOrarioModel> implements OnInit {

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parProfiloOrarioService: ParProfiloOrarioService,
    private refresherService: RefresherService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string {
    return "Profilo Orario";
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  LoadData = (): Observable<Par_ProfiloOrarioModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request = new GenericRequest<Par_ProfiloOrario_GetInModel>(Par_ProfiloOrario_GetInModel);
      request.data.id = state.id;

      return this.parProfiloOrarioService.Par_ProfiloOrarioGet(request).pipe(
        map((res) => res.data.par_ProfiloOrario),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Par_ProfiloOrarioModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();

        subscriber.next(new Par_ProfiloOrarioModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_ProfiloOrarioModel): Observable<boolean> => {
    //let request = new GenericRequest<Par_ProfiloOrario_PutInModel>(Par_ProfiloOrario_PutInModel)


    let request: GenericRequest<Par_ProfiloOrario_PutInModel> =
      new GenericRequest<Par_ProfiloOrario_PutInModel>(Par_ProfiloOrario_PutInModel);
    request.data.par_ProfiloOrario = editModel;

    return this.parProfiloOrarioService.Par_ProfiloOrarioPut(request).pipe(
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
