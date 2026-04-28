import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_Orario_GetInModel, Par_Orario_PutInModel, Par_OrarioModel, OrarioTimbratureTipo } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_OrarioIntervalloHH_Arrange_Coppie_InModel, Par_OrarioIntervalloHHModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-orario-dettaglio-orario-intervallo-hhdialog/edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';
import { ParOrarioIntervalloHHService } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/par-orario-intervallo-hh.service';
import { Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Az_SubCommessaAttivita_4FullListModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/Models/az-subcommessa-attivita-model';



@Component({
  selector: 'app-orari-edit-page',
  templateUrl: './orari-edit-page.component.html',
  styleUrls: ['./orari-edit-page.component.scss'],
  standalone: false
})
export class OrariEditPageComponent extends BasePageConfirmCancelComponent<Par_OrarioModel> implements OnInit {

  public par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  public currSection: string = "sez1";
  public btnEdit: ButtonItem;
  public TMP_counter: number = 0;
  public par_CausaliModelList: Par_CausaliModel[] = [];
  public az_SubCommessaAttivita_4FullList: Az_SubCommessaAttivita_4FullListModel[] = [];
  public orarioTimbratureTipoEnum = OrarioTimbratureTipo;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parOrarioService: ParOrarioService,
    private parOrarioIntervalloHHService: ParOrarioIntervalloHHService,
    private modalCtrl: ModalController,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
  ) {
    super(navCtrl, userInterfaceService, fb);

    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

  }

  get Title(): string {
    return "Orario";
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      numeroCoppie: [null, [Validators.required, Validators.min(1)]],
      idCausale_HH_Lav_MonteOre: [null],
      timbratureTipo: [null, [Validators.required]],
      hh_Teo_MonteOre: [null],
      //idAz_SubCommessaAttivita_MonteOre: [null],
      
    });
  }

  LoadData = (): Observable<Par_OrarioModel | null> => {
    const state = history.state;

    this.par_CausaliModelList = this.sharedParameterGestionePresenzeService.Par_Causali;
    this.az_SubCommessaAttivita_4FullList = this.sharedParameterGestionePresenzeService.Az_SubCommessaAttivita_4Full;

    if (state) {

        let request = new GenericRequest<Par_Orario_GetInModel>(Par_Orario_GetInModel);
        request.data.id = state.id;

        return this.parOrarioService.Par_OrarioGet(request).pipe(
          map((res) => {
            this.par_OrarioIntervalloHH = res.data.par_OrarioIntervalloHH;
            return res.data.par_Orario;
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

  SaveData = (editModel: Par_OrarioModel): Observable<boolean> => {
    //let request = new GenericRequest<Par_Orario_PutInModel>(Par_Orario_PutInModel)


    let request: GenericRequest<Par_Orario_PutInModel> =
      new GenericRequest<Par_Orario_PutInModel>(Par_Orario_PutInModel);
    request.data.par_Orario = editModel;
    request.data.par_OrarioIntervalloHH = this.par_OrarioIntervalloHH;

    return this.parOrarioService.Par_OrarioPut(request).pipe(
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

  get isSez2Visible(): boolean {
    return this._editForm?.get('timbratureTipo')?.value === OrarioTimbratureTipo.IntervalloOrario;
  }

  get isSez3Visible(): boolean {
    const v = this._editForm?.get('timbratureTipo')?.value;
    return v === OrarioTimbratureTipo.MonteOre || v === OrarioTimbratureTipo.MonteOreValore;
  }

  public get_Par_OrarioIntervalloHH(): Par_OrarioIntervalloHHModel[] {
    if (!this.par_OrarioIntervalloHH) {
      return [];
    }
    let retVal = this.par_OrarioIntervalloHH
      .sort((a, b) => a.numCoppia - b.numCoppia);

    if (retVal.length > 1) {
      var c = 0;
    }

    return retVal;
  }

  handleButtonEditClick = (par_OrarioIntervalloHHModel: Par_OrarioIntervalloHHModel) => {
    this.EdiProfiloOrarioDettDialog_Open(par_OrarioIntervalloHHModel);
  }

  public onRangeChange(event: any): void {

    const newValue = event.detail.value;
    this.coppieArrange(newValue);

  }

  public coppieArrange(newValue: number): void {

    let request = new GenericRequest<Par_OrarioIntervalloHH_Arrange_Coppie_InModel>(Par_OrarioIntervalloHH_Arrange_Coppie_InModel);
    request.data.id = this._editModel.id;
    request.data.numCoppie = newValue;
    request.data.par_OrarioIntervalloHH = this.par_OrarioIntervalloHH;
    
    

    this.parOrarioIntervalloHHService.Par_OrarioIntervalloHH_Arrange_NumCoppie(request).pipe(
      map((res) => {
        this.par_OrarioIntervalloHH = res.data.par_OrarioIntervalloHH;
      }),
      catchError((error) => {
        console.error('Errore durante il caricamento dei dati:', error);
        return [null];
      })
    ).subscribe();

  }

  async EdiProfiloOrarioDettDialog_Open(par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel) {


    const modal = await this.modalCtrl.create({
      component: EditParOrarioDettaglioOrarioIntervalloHHDialogComponent,
      componentProps: {
        par_OrarioIntervalloHH: par_OrarioIntervalloHH
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Par_OrarioIntervalloHHModel | null>();

    if (role === 'confirm' && data) {

      const index = this.par_OrarioIntervalloHH.findIndex(p => p.id === data.id);

      if (index > -1) {
        this.par_OrarioIntervalloHH[index] = data;
      } else {
        this.par_OrarioIntervalloHH.push(data);
      }

    }
  }


}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};
