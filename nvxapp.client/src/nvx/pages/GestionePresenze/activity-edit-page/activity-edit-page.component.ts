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
import { Az_AttivitaModel, Az_AttivitaGetInModel, Az_AttivitaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Attivita/Models/az-attivita-model';
import { AzAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Az_Attivita/az-attivita.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-activity-edit-page',
  templateUrl: './activity-edit-page.component.html',
  styleUrls: ['./activity-edit-page.component.scss'],
  standalone: false
})
export class ActivityEditPageComponent extends BasePageConfirmCancelComponent<Az_AttivitaModel> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private azAttivitaService: AzAttivitaService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Attività'; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      textColor: [null, []],
    });
  }

  LoadData = (): Observable<Az_AttivitaModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_AttivitaGetInModel> = new GenericRequest<Az_AttivitaGetInModel>(Az_AttivitaGetInModel);
      request.data.id = state.id;
      return this.azAttivitaService.Az_AttivitaGet(request).pipe(
        map((res) => res.data.az_Attivita),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Az_AttivitaModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();
        subscriber.next(new Az_AttivitaModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_AttivitaModel): Observable<boolean> => {
    let request: GenericRequest<Az_AttivitaPutInModel> =
      new GenericRequest<Az_AttivitaPutInModel>(Az_AttivitaPutInModel);
    request.data.az_Attivita = editModel;
    return this.azAttivitaService.Az_AttivitaPut(request).pipe(
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
