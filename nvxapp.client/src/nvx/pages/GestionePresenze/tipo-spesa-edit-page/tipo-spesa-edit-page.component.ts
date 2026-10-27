import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Par_TipoSpesaGetInModel, Par_TipoSpesaModel, Par_TipoSpesaPutInModel, TIPO_SPESA_TIPO_TESTO, TipoSpesaTipo } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-tipo-spesa-edit-page',
  templateUrl: './tipo-spesa-edit-page.component.html',
  styleUrls: ['./tipo-spesa-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TipoSpesaEditPageComponent extends BasePageConfirmCancelComponent<Par_TipoSpesaModel> {

  public readonly tipi = [TipoSpesaTipo.Generica, TipoSpesaTipo.RimborsoKm];
  public readonly tipoTesto = TIPO_SPESA_TIPO_TESTO;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parTipoSpesaService: ParTipoSpesaService,
    private refresherService: RefresherService,
    private userNavigationService: UserNavigationService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.tipoSpesaEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      tipo: [TipoSpesaTipo.Generica, [Validators.required]],
    });
  }

  LoadData = (): Observable<Par_TipoSpesaModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request = new GenericRequest<Par_TipoSpesaGetInModel>(Par_TipoSpesaGetInModel);
      request.data.id = state.id;

      return this.parTipoSpesaService.Par_TipoSpesaGet(request).pipe(
        map((res) => res.data.par_TipoSpesa),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Par_TipoSpesaModel | null>((subscriber) => {
        subscriber.next(new Par_TipoSpesaModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_TipoSpesaModel): Observable<boolean> => {
    let request = new GenericRequest<Par_TipoSpesaPutInModel>(Par_TipoSpesaPutInModel);
    request.data.par_TipoSpesa = editModel;

    return this.parTipoSpesaService.Par_TipoSpesaPut(request).pipe(
      map(() => {
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };
}
