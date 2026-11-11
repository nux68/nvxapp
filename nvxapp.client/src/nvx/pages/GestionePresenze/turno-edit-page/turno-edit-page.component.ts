import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TurniService } from '../../../ClientServer-Service/GestionePresenze/Turni/turni.service';
import { Par_TurnoModel, Par_Turno_GetInModel, Par_Turno_PutInModel } from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_OrarioModel, Par_Orario_GetAllInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';

// Modifica di un modello di turno (AT-TURNI.1): codice, descrizione, orario (vuoto = riposo), colore
@Component({
  selector: 'app-turno-edit-page',
  templateUrl: './turno-edit-page.component.html',
  styleUrls: ['./turno-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TurnoEditPageComponent extends BasePageConfirmCancelComponent<Par_TurnoModel> {

  public orari: Par_OrarioModel[] = [];

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private turniService: TurniService,
    private parOrarioService: ParOrarioService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.turnoEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      idPar_Orario: [null],
      colore: [null, [Validators.maxLength(7)]],
    });
  }

  LoadData = (): Observable<Par_TurnoModel | null> => {
    const request = new GenericRequest<Par_Turno_GetInModel>(Par_Turno_GetInModel);
    request.data.id = history.state?.id ?? 0;
    return forkJoin({
      turno: this.turniService.Par_TurnoGet(request),
      orari: this.parOrarioService.GetAll(new GenericRequest<Par_Orario_GetAllInModel>(Par_Orario_GetAllInModel))
    }).pipe(
      map(({ turno, orari }) => {
        this.orari = orari.data?.par_Orario ?? [];
        return turno.data?.par_Turno ?? null;
      }),
      catchError(() => of(null))
    );
  };

  SaveData = (editModel: Par_TurnoModel): Observable<boolean> => {
    const request = new GenericRequest<Par_Turno_PutInModel>(Par_Turno_PutInModel);
    request.data.par_Turno = editModel;
    return this.turniService.Par_TurnoPut(request).pipe(
      map(res => res.success),
      catchError(() => of(false))
    );
  };

  // con un errore (es. codice duplicato) si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
