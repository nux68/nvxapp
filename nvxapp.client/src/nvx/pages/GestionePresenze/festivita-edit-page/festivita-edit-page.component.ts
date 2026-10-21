import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParFestivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/par-festivita.service';
import { Par_Festivita_GetInModel, Par_Festivita_PutInModel, Par_FestivitaModel, TipoFestivita } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/Models/par-festivita-model';

@Component({
  selector: 'app-festivita-edit-page',
  templateUrl: './festivita-edit-page.component.html',
  styleUrls: ['./festivita-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FestivitaEditPageComponent extends BasePageConfirmCancelComponent<Par_FestivitaModel> {

  public tipoFestivitaEnum = TipoFestivita;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parFestivitaService: ParFestivitaService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "Festività"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      tipoFestivita: [TipoFestivita.DataRicorrente, [Validators.required]],
      data: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Par_FestivitaModel | null> => {
    const request = new GenericRequest<Par_Festivita_GetInModel>(Par_Festivita_GetInModel);
    request.data.id = history.state?.id ?? 0;

    return this.parFestivitaService.Par_FestivitaGet(request).pipe(
      map((res) => res.data.par_Festivita),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: Par_FestivitaModel): Observable<boolean> => {
    // il selettore restituisce anche l'ora: si salva solo la data, senza fuso orario
    if (editModel.data)
      editModel.data = editModel.data.substring(0, 10) + 'T00:00:00';

    const request = new GenericRequest<Par_Festivita_PutInModel>(Par_Festivita_PutInModel);
    request.data.par_Festivita = editModel;

    return this.parFestivitaService.Par_FestivitaPut(request).pipe(
      map(() => true),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };
}
