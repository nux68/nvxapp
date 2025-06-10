import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { Az_SediModel, Az_SediGetInModel, Az_SediPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';

@Component({
  selector: 'app-az-sedi-edit-page',
  templateUrl: './az-sedi-edit-page.component.html',
  styleUrls: ['./az-sedi-edit-page.component.scss'],
  standalone: false
})
export class AzSediEditPageComponent extends BasePageConfirmCancelComponent<Az_SediModel> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private azSediService: AzSediService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Sede'; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      default: [false]
    });
  }

  LoadData = (): Observable<Az_SediModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_SediGetInModel> = new GenericRequest<Az_SediGetInModel>(Az_SediGetInModel);
      request.data.id = state.id;
      return this.azSediService.AzSediGet(request).pipe(
        map((res) => {
          return res.data.az_Sedi;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Az_SediModel | null>((subscriber) => {
        subscriber.next(new Az_SediModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_SediModel): Observable<boolean> => {
    let request: GenericRequest<Az_SediPutInModel> =
      new GenericRequest<Az_SediPutInModel>(Az_SediPutInModel);
    request.data.az_Sedi = editModel;
    return this.azSediService.AzSediPut(request).pipe(
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
