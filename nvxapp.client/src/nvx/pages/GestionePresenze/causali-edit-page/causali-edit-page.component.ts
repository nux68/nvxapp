import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Par_CausaliGetInModel, Par_CausaliModel, Par_CausaliPutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-causali-edit-page',
  templateUrl: './causali-edit-page.component.html',
  styleUrls: ['./causali-edit-page.component.scss'],
  standalone: false
})
export class CausaliEditPageComponent extends BasePageConfirmCancelComponent<Par_CausaliModel> {

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parCausaliService: ParCausaliService,
    private refresherService: RefresherService,
    private userNavigationService: UserNavigationService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "Causale"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      codice: [null, [Validators.required, Validators.maxLength(5)]],
    });
  }

  LoadData = (): Observable<Par_CausaliModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request = new GenericRequest<Par_CausaliGetInModel>(Par_CausaliGetInModel);
      request.data.id = state.id;

      return this.parCausaliService.Par_CausaliGet(request).pipe(
        map((res) => res.data.par_Causale),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Par_CausaliModel | null>((subscriber) => {
        subscriber.next(new Par_CausaliModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_CausaliModel): Observable<boolean> => {
    let request = new GenericRequest<Par_CausaliPutInModel>(Par_CausaliPutInModel);
    request.data.par_Causale = editModel;

    return this.parCausaliService.Par_CausaliPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };
}
