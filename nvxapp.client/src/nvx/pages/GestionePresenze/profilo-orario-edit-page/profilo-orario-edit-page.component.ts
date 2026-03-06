import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_ProfiloOrarioModel, Par_ProfiloOrario_GetInModel, Par_ProfiloOrario_PutInModel, StraoTipoConteggio, TipoProfilo } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { ParProfiloOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Par_ProfiloOrarioGG_Arrange_NumDay_InModel, Par_ProfiloOrarioGGModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { EditParProfiloOrarioDettaglioOrarioDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-profilo-orario-dettaglio-orario-dialog/edit-par-profilo-orario-dettaglio-orario-dialog.component';
import { Par_OrarioIntervalloHHModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { state } from '@angular/animations';
import { ParProfiloOrarioGGService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrarioGG/par-profilo-orario-gg.service';

@Component({
  selector: 'app-profilo-orario-edit-page',
  templateUrl: './profilo-orario-edit-page.component.html',
  styleUrls: ['./profilo-orario-edit-page.component.scss'],
  standalone: false
})
export class ProfiloOrarioEditPageComponent extends BasePageConfirmCancelComponent<Par_ProfiloOrarioModel> implements OnInit {

  public currSection: string = "sez1";
  public par_OrarioModelList: Par_OrarioModel[] = [];
  public par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[];
  public par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public btnAdd: ButtonItem;
  public TMP_counter: number = 0;
  public tipoProfiloEnum = TipoProfilo;
  public straoTipoConteggioEnum = StraoTipoConteggio; 
  
  

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private parProfiloOrarioService: ParProfiloOrarioService,
              private parProfiloOrarioGGService: ParProfiloOrarioGGService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private modalCtrl: ModalController,
              public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(navCtrl, userInterfaceService, fb);
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.btnAdd = userInterfaceService.Btn_Aggiungi;
    this.btnAdd.event = this.handleButtonAddClick;
  }

  get Title(): string {     return "Profilo Orario"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      numGiorniCiclo: [0, [Validators.required, Validators.min(1)]],
      tipoProfilo: [0, [Validators.required ]],
      idPar_Orario_Festivo: [0, [Validators.required]],

      straoSogliaHHFullTime: [0, [Validators.required]],
      straoTipoConteggio: [0, [Validators.required]],
      supplTipoConteggio: [0, [Validators.required]]
      
    });
  }

  LoadData = (): Observable<Par_ProfiloOrarioModel | null> => {
    const state = history.state;

    this.par_OrarioModelList = this.sharedParameterGestionePresenzeService.Par_Orario;
    this.par_OrarioIntervalloHH = this.sharedParameterGestionePresenzeService.Par_OrarioIntervalloHH;

    if (state) {
        let request = new GenericRequest<Par_ProfiloOrario_GetInModel>(Par_ProfiloOrario_GetInModel);
        request.data.id = state.id;

        return this.parProfiloOrarioService.Par_ProfiloOrarioGet(request).pipe(
          map((res) => {
            this.par_ProfiloOrarioGG = res.data.par_ProfiloOrarioGG;
            return res.data.par_ProfiloOrario;
          }),
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

  SaveData = (editModel: Par_ProfiloOrarioModel): Observable<boolean> => {

    let request: GenericRequest<Par_ProfiloOrario_PutInModel> =
    new GenericRequest<Par_ProfiloOrario_PutInModel>(Par_ProfiloOrario_PutInModel);
    request.data.par_ProfiloOrario = editModel;

    //elimino i contatori temporanei
    this.par_ProfiloOrarioGG = this.par_ProfiloOrarioGG.map(x => {
      if (x.id < 0)
        x.id = 0;
      return x;
    })


    request.data.par_ProfiloOrarioGG = this.par_ProfiloOrarioGG;

    return this.parProfiloOrarioService.Par_ProfiloOrarioPut(request).pipe(
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

  getDayMock(): any {

    let v= Array(this._editForm.get('numGiorniCiclo')?.value).fill(0);

    return v;
    

  }

  getDays(): number {

    if (this._editForm.get('tipoProfilo')?.value == 0) {
      return 7;
    }
    else {
      return 15;
    }
    

  }

  /**
   * 
   * @param newValue determina il numero di giorni del profilo
   * @param numGiorno_Incrementa , indica il numero di giorno sul quale agire, se positico aginge una riga, se negativo la cancella
   */
  public daysArrange(newValue: number, numGiorno_Incrementa: number): void {

    let request = new GenericRequest<Par_ProfiloOrarioGG_Arrange_NumDay_InModel>(Par_ProfiloOrarioGG_Arrange_NumDay_InModel);
    request.data.id = this._editModel.id;
    request.data.numGiorniCiclo = newValue;
    request.data.par_ProfiloOrarioGG = this.par_ProfiloOrarioGG;

    if (numGiorno_Incrementa != 0) {
      request.data.numGiorno_Incrementa = numGiorno_Incrementa
    }

    this.parProfiloOrarioGGService.Par_ProfiloOrarioGG_Arrange_NumDay(request).pipe(
      map((res) => {
        this.par_ProfiloOrarioGG = res.data.par_ProfiloOrarioGG;
      }),
      catchError((error) => {
        console.error('Errore durante il caricamento dei dati:', error);
        return [null];
      })
    ).subscribe();

  }

  public onDayRangeChange(event: any): void {

    const newValue = event.detail.value;
    this.daysArrange(newValue, 0)

  }

  /**
   * Serve per fornire le righe per la descrizione degli intervalli orari dell'orario selezionato nel giorno
   * @param idPar_Orario 
   * @returns
   */
  public get_par_OrarioIntervalloHH(idPar_Orario: number): Par_OrarioIntervalloHHModel[] {

    return  this.par_OrarioIntervalloHH.filter(x => x.idPar_Orario == idPar_Orario).sort(x => x.numCoppia);

  }

  /**
   * Ottiene le righe di par_ProfiloOrarioGG per il giorno selezionato
   * @param day
   * @returns
   */
  public get_par_ProfiloOrarioGG_4Day(day: number): Par_ProfiloOrarioGGModel[] {
    if (!this.par_ProfiloOrarioGG) {
      return [];
    }
    let retVal =  this.par_ProfiloOrarioGG.filter(g => g.numGiorno === day)
                                          .sort((a, b) => a.zOrder - b.zOrder);

    return retVal;
  }

  handleButtonEditClick = (par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel) => {
    this.EdiProfiloOrarioDettDialog_Open(par_ProfiloOrarioGG);
  }

  handleButtonAddClick = (giorno: any) => {
    this.daysArrange(this._editForm.get('numGiorniCiclo')?.value, giorno)
  }
  
  handleButtonDeleteClick = async (giorno: any) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione dell  Orario');
    if (result) {
      this.daysArrange(this._editForm.get('numGiorniCiclo')?.value, (giorno * -1) )
    }
  }

  async EdiProfiloOrarioDettDialog_Open(par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel) {

  
    const modal = await this.modalCtrl.create({
      component: EditParProfiloOrarioDettaglioOrarioDialogComponent,
      componentProps: {
          par_ProfiloOrarioGG: par_ProfiloOrarioGG
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Par_ProfiloOrarioGGModel | null>();

    if (role === 'confirm' && data) {

      const index = this.par_ProfiloOrarioGG.findIndex(p => p.id === data.id);

      if (index > -1) {
        this.par_ProfiloOrarioGG[index] = data;
      } else {
        this.par_ProfiloOrarioGG.push(data);
      }

    }
  }

  public onTipoProfiloChange(event: any): void {

    if (this._editForm.get('tipoProfilo')?.value == 0) {

      this._editForm.patchValue({
        numGiorniCiclo: 7
      });

      this.daysArrange(7,0);

    }

  }
  

}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};


