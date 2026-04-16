import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { NavController, ModalController } from '@ionic/angular';
import { Observable, map, catchError } from 'rxjs';
import { Par_OrarioModel, Par_Orario_GetInModel, Par_Orario_PutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_OrarioIntervalloHHModel, Par_OrarioIntervalloHH_Arrange_Coppie_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { ParOrarioIntervalloHHService } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/par-orario-intervallo-hh.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-orario-dettaglio-orario-intervallo-hhdialog/edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ParExportCauService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Par_ExportCau_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/Models/par-export-cau-causali-model';
import { Par_ExportCau_Get_InModel, Par_ExportCau_Put_InModel, Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';

@Component({
  selector: 'app-export-cau-edit-page',
  templateUrl: './export-cau-edit-page.component.html',
  styleUrls: ['./export-cau-edit-page.component.scss'],
  standalone: false
})
export class ExportCauEditPageComponent extends BasePageConfirmCancelComponent<Par_ExportCauModel> implements OnInit {

  public par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  public currSection: string = "sez1";
  public btnEdit: ButtonItem;
  public TMP_counter: number = 0;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parExportCauService: ParExportCauService,
    private parOrarioIntervalloHHService: ParOrarioIntervalloHHService,
    private modalCtrl: ModalController,
  ) {
    super(navCtrl, userInterfaceService, fb);

    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

  }

  get Title(): string {return "Modelli export";}

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  LoadData = (): Observable<Par_ExportCauModel | null> => {
    const state = history.state;

    if (state) {

      let request = new GenericRequest<Par_ExportCau_Get_InModel>(Par_ExportCau_Get_InModel);
      request.data.id = state.id;

      return this.parExportCauService.Par_ExportCauGet(request).pipe(
        map((res) => {
          this.par_ExportCau_Causali = res.data.par_ExportCau_Causali;
          return res.data.par_ExportCau;
        }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return [null];
        })
      );
    }
    else {
      return null;
    }

  };

  SaveData = (editModel: Par_ExportCauModel): Observable<boolean> => {
    


    let request: GenericRequest<Par_ExportCau_Put_InModel> = new GenericRequest<Par_ExportCau_Put_InModel>(Par_ExportCau_Put_InModel);
    request.data.par_ExportCau = editModel;
    request.data.par_ExportCau_Causali = this.par_ExportCau_Causali;

    return this.parExportCauService.Par_ExportCauPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante il salvataggio:', error);
        return [false];
      })
    );
  };

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

  public get_par_ExportCau_Causali(): Par_ExportCau_CausaliModel[] {
    if (!this.par_ExportCau_Causali) {
      return [];
    }

    //let retVal = this.par_ExportCau_Causali.sort((a, b) => a.numCoppia - b.numCoppia);

    let retVal = this.par_ExportCau_Causali;
    

    return retVal;
  }

  handleButtonEditClick = (par_OrarioIntervalloHHModel: Par_OrarioIntervalloHHModel) => {
    this.EdiProfiloOrarioDettDialog_Open(par_OrarioIntervalloHHModel);
  }

  //public onRangeChange(event: any): void {

  //  const newValue = event.detail.value;
  //  this.coppieArrange(newValue);

  //}

  //public coppieArrange(newValue: number): void {

  //  let request = new GenericRequest<Par_OrarioIntervalloHH_Arrange_Coppie_InModel>(Par_OrarioIntervalloHH_Arrange_Coppie_InModel);
  //  request.data.id = this._editModel.id;
  //  request.data.numCoppie = newValue;
  //  request.data.par_OrarioIntervalloHH = this.par_OrarioIntervalloHH;



  //  this.parOrarioIntervalloHHService.Par_OrarioIntervalloHH_Arrange_NumCoppie(request).pipe(
  //    map((res) => {
  //      this.par_OrarioIntervalloHH = res.data.par_OrarioIntervalloHH;
  //    }),
  //    catchError((error) => {
  //      console.error('Errore durante il caricamento dei dati:', error);
  //      return [null];
  //    })
  //  ).subscribe();

  //}

  async EdiProfiloOrarioDettDialog_Open(par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel) {


    //const modal = await this.modalCtrl.create({
    //  component: EditParOrarioDettaglioOrarioIntervalloHHDialogComponent,
    //  componentProps: {
    //    par_OrarioIntervalloHH: par_OrarioIntervalloHH
    //  },
    //});

    //await modal.present();

    //const { data, role } = await modal.onWillDismiss<Par_OrarioIntervalloHHModel | null>();

    //if (role === 'confirm' && data) {

    //  const index = this.par_OrarioIntervalloHH.findIndex(p => p.id === data.id);

    //  if (index > -1) {
    //    this.par_OrarioIntervalloHH[index] = data;
    //  } else {
    //    this.par_OrarioIntervalloHH.push(data);
    //  }

    //}

  }


}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};
