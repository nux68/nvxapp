import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { Par_AttivitaModel, Par_AttivitaGetInModel, Par_AttivitaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { ParAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-activity-edit-page',
  templateUrl: './activity-edit-page.component.html',
  styleUrls: ['./activity-edit-page.component.scss'],
  standalone: false
})
export class ActivityEditPageComponent extends BasePageConfirmCancelComponent<Par_AttivitaModel> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private azAttivitaService: ParAttivitaService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Attività'; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      backgroundColor: [null, []],
      textColor: [null, []],
    });
  }

  LoadData = (): Observable<Par_AttivitaModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Par_AttivitaGetInModel> = new GenericRequest<Par_AttivitaGetInModel>(Par_AttivitaGetInModel);
      request.data.id = state.id;
      return this.azAttivitaService.Par_AttivitaGet(request).pipe(
        map((res) => res.data.par_Attivita),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Par_AttivitaModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();
        subscriber.next(new Par_AttivitaModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_AttivitaModel): Observable<boolean> => {
    let request: GenericRequest<Par_AttivitaPutInModel> =
      new GenericRequest<Par_AttivitaPutInModel>(Par_AttivitaPutInModel);
    request.data.par_Attivita = editModel;
    return this.azAttivitaService.Par_AttivitaPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };
}

const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  return null;
};
