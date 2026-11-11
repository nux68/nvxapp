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
import { Az_SediRepartoTurno_GetInModel, Az_SediRepartoTurno_PutInModel, Par_TurnoModel, Par_Turno_GetAllInModel } from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoModel, Az_SediReparto_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';

/** riga della pagina: un turno, se usato nel reparto, con la copertura minima e i giorni in cui vale */
export class TurnoCoperturaRiga {
  public turno: Par_TurnoModel;
  public usato: boolean = false;
  public minPersone: number = 0;
  public giorni: boolean[] = [true, true, true, true, true, true, true];
}

export class TurnoCoperturaEdit {
  public idAz_SediReparto: number | null = null;
}

// Turni usati in un reparto e copertura minima per turno (AT-TURNI.1)
@Component({
  selector: 'app-turno-coperture-page',
  templateUrl: './turno-coperture-page.component.html',
  styleUrls: ['./turno-coperture-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TurnoCoperturePageComponent extends BasePageConfirmCancelComponent<TurnoCoperturaEdit> {

  public reparti: Az_SediRepartoModel[] = [];
  public turni: Par_TurnoModel[] = [];
  public righe: TurnoCoperturaRiga[] = [];
  public readonly giorniSettimana = [0, 1, 2, 3, 4, 5, 6];

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private turniService: TurniService,
    private azSediRepartoService: AzSediRepartoService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.turnoCoperture.title'); }

  get EditForm(): FormGroup {
    const form = this.fb.group({
      idAz_SediReparto: [null as number | null, [Validators.required]],
    });
    form.get('idAz_SediReparto')!.valueChanges.subscribe(id => this.caricaReparto(id));
    return form;
  }

  nomeGiorno(i: number): string {
    return this.transloco.translate('attendance.pages.turni.wd' + i);
  }

  LoadData = (): Observable<TurnoCoperturaEdit | null> => {
    return forkJoin({
      reparti: this.azSediRepartoService.GetAll(new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel)),
      turni: this.turniService.GetAll(new GenericRequest<Par_Turno_GetAllInModel>(Par_Turno_GetAllInModel))
    }).pipe(
      map(({ reparti, turni }) => {
        this.reparti = reparti.data?.az_SediReparto ?? [];
        this.turni = turni.data?.par_Turno ?? [];
        const edit = new TurnoCoperturaEdit();
        edit.idAz_SediReparto = history.state?.idAz_SediReparto ?? (this.reparti.length === 1 ? this.reparti[0].id : null);
        return edit;
      }),
      catchError(() => of(null))
    );
  };

  private caricaReparto(idAz_SediReparto: number | null) {
    this.righe = [];
    if (!idAz_SediReparto)
      return;
    const request = new GenericRequest<Az_SediRepartoTurno_GetInModel>(Az_SediRepartoTurno_GetInModel);
    request.data.idAz_SediReparto = idAz_SediReparto;
    this.turniService.Coperture4Reparto(request).subscribe(res => {
      const esistenti = res.data?.az_SediRepartoTurno ?? [];
      this.righe = this.turni.map(t => {
        const riga = new TurnoCoperturaRiga();
        riga.turno = t;
        const e = esistenti.find(x => x.idPar_Turno === t.id);
        if (e) {
          riga.usato = true;
          riga.minPersone = e.minPersone;
          riga.giorni = (e.giorniSettimana ?? '1111111').split('').map(c => c === '1');
        }
        return riga;
      });
    });
  }

  SaveData = (editModel: TurnoCoperturaEdit): Observable<boolean> => {
    const request = new GenericRequest<Az_SediRepartoTurno_PutInModel>(Az_SediRepartoTurno_PutInModel);
    request.data.idAz_SediReparto = editModel.idAz_SediReparto ?? 0;
    request.data.az_SediRepartoTurno = this.righe.filter(r => r.usato).map(r => ({
      id: 0,
      idAz_SediReparto: request.data.idAz_SediReparto,
      idPar_Turno: r.turno.id,
      minPersone: Math.max(0, Number(r.minPersone) || 0),
      giorniSettimana: r.giorni.map(g => g ? '1' : '0').join('')
    }));
    return this.turniService.Coperture4RepartoPut(request).pipe(
      map(res => res.success),
      catchError(() => of(false))
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
