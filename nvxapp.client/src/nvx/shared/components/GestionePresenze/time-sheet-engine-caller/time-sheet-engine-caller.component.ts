import { Component, Input, OnInit } from '@angular/core';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable } from 'rxjs';
import { of } from 'rxjs/internal/observable/of';

@Component({
  selector: 'app-time-sheet-engine-caller',
  templateUrl: './time-sheet-engine-caller.component.html',
  styleUrls: ['./time-sheet-engine-caller.component.scss'],
  standalone: false
})
export class TimeSheetEngineCallerComponent extends BaseDialogConfirmCancelComponent<TimeSheetEngineCallerData> {


  @Input() timeSheetEngineCallerData: TimeSheetEngineCallerData;

  public currSection: string = "sez1";

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  get Title(): string { return "Calcola"; }

  LoadData = (): Observable<TimeSheetEngineCallerData | null> => {

    return of(this.timeSheetEngineCallerData);
  };

  SaveData = (editModel: TimeSheetEngineCallerData): Observable<TimeSheetEngineCallerData> => {
    return of(this._editModel);
  };

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [ Validators.maxLength(10)]],
      //idPar_ProfiloOrario: [this.dip_ProfiloOrario.idPar_ProfiloOrario, [Validators.required, Validators.min(1)]],
      //numGiornoPartenzaCiclo: [null, [Validators.required, Validators.min(1), Validators.max(this.getDayProf())]],
      //dal: [this.dip_ProfiloOrario.idPar_ProfiloOrario, [Validators.required]],
      //al: [this.dip_ProfiloOrario.idPar_ProfiloOrario, [Validators.required]],
    });

  }

  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void { }
  onSedeChanged(sediId: number | undefined): void {
    
  }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

}


export class TimeSheetEngineCallerData {
  public initialSelectedUserId: string | string[] | null = null

  constructor() {
    this.initialSelectedUserId = null;
  }


}



