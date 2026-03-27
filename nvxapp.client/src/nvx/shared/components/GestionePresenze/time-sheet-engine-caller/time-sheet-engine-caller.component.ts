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
      dal: [null, [Validators.required]],
      al: [null, [Validators.required]],

      approva_Richieste_Timbrature: [null, [Validators.required]],
      approva_Richieste_Giustificativo: [null, [Validators.required]],
      genera_Timbrature_Mancanti: [null, [Validators.required]],
      genera_Giustificativo_Assenza: [null, [Validators.required]],

      
    });

  }

  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void {}
  onSedeChanged(sediId: number | undefined): void {
    
  }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.timeSheetEngineCallerData.currSelectedUserId = userIds;
}

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

}


export class TimeSheetEngineCallerData {
  // imposta il valore iniziale degli user selezionati
  public initialSelectedUserId: string | string[] | null = null

  // contiene la selezione degli user dopo la conferma
  public currSelectedUserId: string | string[] | null = null
  dal: string
  al: string;
  approva_Richieste_Timbrature: boolean
  approva_Richieste_Giustificativo: boolean
  genera_Timbrature_Mancanti: boolean
  genera_Giustificativo_Assenza: boolean

  constructor() {
    this.initialSelectedUserId = null;
    this.dal = "";
    this.al = "";
  }


}



