import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { Par_ProfiloOrarioGGModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { Par_OrarioIntervalloHHModel, RoundDirection, TimeRoundInterval } from '../../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { StringHelperService } from '../../../../Utility/infrastructure/string-helper.service';
import { Par_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';

@Component({
  selector: 'app-edit-par-orario-dettaglio-orario-intervallo-hhdialog',
  templateUrl: './edit-par-orario-dettaglio-orario-intervallo-hhdialog.component.html',
  styleUrls: ['./edit-par-orario-dettaglio-orario-intervallo-hhdialog.component.scss'],
  standalone: false
})


export class EditParOrarioDettaglioOrarioIntervalloHHDialogComponent extends BaseDialogConfirmCancelComponent<Par_OrarioIntervalloHHModel> {

  @Input() par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel;


  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  public timeRoundIntervalEnum =  TimeRoundInterval;
  public roundDirectionEnum = RoundDirection;
  public par_CausaliModelList: Par_CausaliModel[] = [];
  public currSection: string = "sez1";

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    private stringHelperService: StringHelperService,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
  }

  get Title(): string { return "Intervallo orari"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      dalle: [null, [Validators.required]],
      dalle_Limite_DX: [null, [Validators.required]],
      dalle_Limite_SX: [null, [Validators.required]],
      dalle_Arrotondamento: [null, [Validators.required]],
      dalle_Arrotondamento_Verso: [null, [Validators.required]],

      alle: [null, [Validators.required]],
      alle_Limite_DX: [null, [Validators.required]],
      alle_Limite_SX: [null, [Validators.required]],
      alle_Arrotondamento: [null, [Validators.required]],
      alle_Arrotondamento_Verso: [null, [Validators.required]],

      idCausale_HH_Lav: [null, [Validators.required]],

    });
  }

  LoadData = (): Observable<Par_OrarioIntervalloHHModel | null> => {

    const now = new Date();
    this.dateTime = this.stringHelperService.DateCurr_To_ISOString();
    this.formattedDate = this.stringHelperService.Date_To_S_ddmmyyyy(now)
    this.formattedTime = this.stringHelperService.Date_To_S_hhmm(now);
    this.par_CausaliModelList = this.sharedParameterGestionePresenzeService.Par_Causali;

    return of(this.par_OrarioIntervalloHH);
  };

  SaveData = (editModel: Par_OrarioIntervalloHHModel): Observable<Par_OrarioIntervalloHHModel> => {
    return of(this._editModel);
  };

  updateDateTimeDalle(event: any) {

    const old_value_dalle_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.dalle);
    const new_value_dalle_in_S: number = this.stringHelperService.hhmmss_ToSeconds(event.detail.value);
    const dif_dalle = new_value_dalle_in_S - old_value_dalle_in_S;
    this._editModel.dalle = event.detail.value;

    const old_value_dalle_SX_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.dalle_Limite_SX);
    const new_value_dalle_SX_in_S = old_value_dalle_SX_in_S + dif_dalle;
    this._editModel.dalle_Limite_SX = this.stringHelperService.secondsTo_hhmmss(new_value_dalle_SX_in_S);

    const old_value_dalle_DX_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.dalle_Limite_DX);
    const new_value_dalle_DX_in_S = old_value_dalle_DX_in_S + dif_dalle;
    this._editModel.dalle_Limite_DX = this.stringHelperService.secondsTo_hhmmss(new_value_dalle_DX_in_S);
    
    this._editForm.patchValue({
      dalle: this._editModel.dalle,
      dalle_Limite_SX: this._editModel.dalle_Limite_SX,
      dalle_Limite_DX: this._editModel.dalle_Limite_DX
    });

  }

  updateDateTimeAlle(event: any) {
    const old_value_alle_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.alle);
    const new_value_alle_in_S: number = this.stringHelperService.hhmmss_ToSeconds(event.detail.value);
    const dif_alle = new_value_alle_in_S - old_value_alle_in_S;
    this._editModel.alle = event.detail.value;

    const old_value_alle_SX_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.alle_Limite_SX);
    const new_value_alle_SX_in_S = old_value_alle_SX_in_S + dif_alle;
    this._editModel.alle_Limite_SX = this.stringHelperService.secondsTo_hhmmss(new_value_alle_SX_in_S);

    const old_value_alle_DX_in_S: number = this.stringHelperService.hhmmss_ToSeconds(this._editModel.alle_Limite_DX);
    const new_value_alle_DX_in_S = old_value_alle_DX_in_S + dif_alle;
    this._editModel.alle_Limite_DX = this.stringHelperService.secondsTo_hhmmss(new_value_alle_DX_in_S);

    this._editForm.patchValue({
      alle: this._editModel.alle,
      alle_Limite_SX: this._editModel.alle_Limite_SX,
      alle_Limite_DX: this._editModel.alle_Limite_DX
    });
  }

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

}
