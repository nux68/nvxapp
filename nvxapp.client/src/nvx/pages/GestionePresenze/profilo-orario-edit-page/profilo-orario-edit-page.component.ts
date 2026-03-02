import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_ProfiloOrarioModel, Par_ProfiloOrario_GetInModel, Par_ProfiloOrario_PutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { ParProfiloOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Par_ProfiloOrarioGGModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { EditParProfiloOrarioDettaglioOrarioDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-profilo-orario-dettaglio-orario-dialog/edit-par-profilo-orario-dettaglio-orario-dialog.component';

@Component({
  selector: 'app-profilo-orario-edit-page',
  templateUrl: './profilo-orario-edit-page.component.html',
  styleUrls: ['./profilo-orario-edit-page.component.scss'],
  standalone: false
})
export class ProfiloOrarioEditPageComponent extends BasePageConfirmCancelComponent<Par_ProfiloOrarioModel> implements OnInit {

  public currSection: string = "sez1";
  public par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel[];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public btnAdd: ButtonItem;
  public TMP_counter: number = 0;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parProfiloOrarioService: ParProfiloOrarioService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private modalCtrl: ModalController,
  ) {
    super(navCtrl, userInterfaceService, fb);
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.btnAdd = userInterfaceService.Btn_Aggiungi;
    this.btnAdd.event = this.handleButtonAddClick;
  }



  public getUniqueDays(): number[] {
    if (!this.par_ProfiloOrarioGG) {
      return [];
    }
    // Estrae tutti i numGiorno
    const allDays = this.par_ProfiloOrarioGG.map(g => g.numGiorno);
    // Rimuove i duplicati e ordina
    return [...new Set(allDays)].sort((a, b) => a - b);
  }


  public onRangeChange(event: any): void {

        const newValue = event.detail.value;
        const UniqueDays = this.getUniqueDays();

        if (UniqueDays.length != newValue) {
          if (newValue > UniqueDays.length) {

            for (let i: number = UniqueDays.length; i <= newValue; i++) {

              this.par_ProfiloOrarioGG.push(this.init_par_ProfiloOrarioGG(i + 1, 1, this.par_ProfiloOrarioGG[0].idPar_Orario) );
            }
              
            
          }
          else {
            this.par_ProfiloOrarioGG = this.par_ProfiloOrarioGG.filter(g => g.numGiorno <= newValue);
          }
        }
    
  }

  public init_par_ProfiloOrarioGG(numGiorno: number, zOrder: number, idPar_Orario: number): Par_ProfiloOrarioGGModel {

    this.TMP_counter--;

    let _par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel = new Par_ProfiloOrarioGGModel();
    _par_ProfiloOrarioGG.id = this.TMP_counter;
    _par_ProfiloOrarioGG.zOrder = zOrder;
    _par_ProfiloOrarioGG.numGiorno = numGiorno;
    _par_ProfiloOrarioGG.idPar_ProfiloOrario = (this._editModel != null) ? this._editModel.id : 0;
    _par_ProfiloOrarioGG.idPar_Orario = idPar_Orario;

    return _par_ProfiloOrarioGG;
  }



  get Title(): string {
    return "Profilo Orario";
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      numGiorniCiclo: [null, [Validators.required, Validators.min(1) ]],
    });
  }

  LoadData = (): Observable<Par_ProfiloOrarioModel | null> => {
    const state = history.state;

    if (state && state.id) {
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
    } else {
      return new Observable<Par_ProfiloOrarioModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();

        let par_ProfiloOrarioModel = new Par_ProfiloOrarioModel();
        par_ProfiloOrarioModel.codice = "0000";
        par_ProfiloOrarioModel.descrizione = "Nuovo profilo"
        par_ProfiloOrarioModel.numGiorniCiclo = 7;
        this.par_ProfiloOrarioGG = [];

        for (let i: number = 1; i <= par_ProfiloOrarioModel.numGiorniCiclo; i++) {
          this.par_ProfiloOrarioGG.push(this.init_par_ProfiloOrarioGG(i, 1, 1));
        }

        


        subscriber.next(par_ProfiloOrarioModel);
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_ProfiloOrarioModel): Observable<boolean> => {
    //let request = new GenericRequest<Par_ProfiloOrario_PutInModel>(Par_ProfiloOrario_PutInModel)


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

    return 15;

  }

  public get_par_ProfiloOrarioGG(day: number): Par_ProfiloOrarioGGModel[] {
    if (!this.par_ProfiloOrarioGG) {
      return [];
    }
    let retVal =  this.par_ProfiloOrarioGG.filter(g => g.numGiorno === day)
      .sort((a, b) => a.zOrder - b.zOrder);

    if (retVal.length > 1) {
      var c = 0;
    }

    return retVal;
  }

  handleButtonEditClick = (par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel) => {
    this.EdiProfiloOrarioDettDialog_Open(par_ProfiloOrarioGG);
  }

  handleButtonAddClick = (giorno: any) => {

    const last = this.get_par_ProfiloOrarioGG(giorno);
    let zOrder = last[last.length - 1].zOrder + 1;
    let idPar_Orario = last[last.length - 1].idPar_Orario;

    let par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel = this.init_par_ProfiloOrarioGG(giorno, zOrder, this._editModel.id);

    par_ProfiloOrarioGG.idPar_Orario = idPar_Orario;

    this.EdiProfiloOrarioDettDialog_Open(par_ProfiloOrarioGG);
  }

  

  handleButtonDeleteClick = async (item: Par_ProfiloOrarioGGModel) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione dell  Orario');
    if (result) {
      this.par_ProfiloOrarioGG = this.par_ProfiloOrarioGG.filter(g => g.id !== item.id);
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


}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};


