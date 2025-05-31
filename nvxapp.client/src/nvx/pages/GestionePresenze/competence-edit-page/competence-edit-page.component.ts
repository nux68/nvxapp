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
import { Az_CompetenzaModel, Az_CompetenzaGetInModel, Az_CompetenzaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Competenza/Models/az-competenza-model';
import { AzCompetenzaService } from '../../../ClientServer-Service/GestionePresenze/Az_Competenza/az-competenza.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-competence-edit-page',
  templateUrl: './competence-edit-page.component.html',
  styleUrls: ['./competence-edit-page.component.scss'],
  standalone: false
})
export class CompetenceEditPageComponent extends BasePageConfirmCancelComponent<Az_CompetenzaModel> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private azCompetenzaService: AzCompetenzaService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Comtetenze'; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      //codice: [null, [Validators.required, Validators.maxLength(5)]],
      //backgroundColor: [null, []],
      textColor: [null, []],
    });
  }

  LoadData = (): Observable<Az_CompetenzaModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_CompetenzaGetInModel> = new GenericRequest<Az_CompetenzaGetInModel>(Az_CompetenzaGetInModel);
      request.data.id = state.id;
      return this.azCompetenzaService.Az_CompetenzaGet(request).pipe(
        map((res) => res.data.az_Competenza),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Az_CompetenzaModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();
        subscriber.next(new Az_CompetenzaModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_CompetenzaModel): Observable<boolean> => {
    let request: GenericRequest<Az_CompetenzaPutInModel> =
      new GenericRequest<Az_CompetenzaPutInModel>(Az_CompetenzaPutInModel);
    request.data.az_Competenza = editModel;
    return this.azCompetenzaService.Az_CompetenzaPut(request).pipe(
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
