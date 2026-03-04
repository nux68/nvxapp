import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Par_Orario_GetInModel, Par_Orario_PutInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_OrarioIntervalloHHModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-orario-dettaglio-orario-intervallo-hhdialog/edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';


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

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parOrarioService: ParOrarioService,
    private modalCtrl: ModalController,
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
    });
  }

  LoadData = (): Observable<Par_OrarioModel | null> => {
    const state = history.state;

    if (state && state.id ) {
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
    } else {
      return new Observable<Par_OrarioModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();

        let par_OrarioModel: Par_OrarioModel = new Par_OrarioModel()
        par_OrarioModel.codice = "0000";
        par_OrarioModel.descrizione = "Nuovo orario";
        par_OrarioModel.numeroCoppie = 1;
        this.par_OrarioIntervalloHH = [];

        for (let i: number = 1; i <= par_OrarioModel.numeroCoppie; i++) {
          this.par_OrarioIntervalloHH.push(this.init_Par_OrarioIntervalloHH(i));
        }

        subscriber.next(par_OrarioModel);
        subscriber.complete();
      });
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

  getCoppieMock(): any {

    let v = Array(this._editForm.get('numeroCoppie')?.value).fill(0);

    return v;

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
    

    if (this.par_OrarioIntervalloHH.length != newValue) {
      if (newValue > this.par_OrarioIntervalloHH.length) {

        for (let i: number = this.par_OrarioIntervalloHH.length; i < newValue; i++) {
          this.par_OrarioIntervalloHH.push(this.init_Par_OrarioIntervalloHH(i + 1));
        }
      }
      else {
        this.par_OrarioIntervalloHH = this.par_OrarioIntervalloHH.filter(g => g.numCoppia <= newValue);
      }
    }

  }


  public init_Par_OrarioIntervalloHH(numCoppia: number): Par_OrarioIntervalloHHModel {

    this.TMP_counter--;

    let _par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel = new Par_OrarioIntervalloHHModel();
    _par_OrarioIntervalloHH.numCoppia = numCoppia;

    switch (numCoppia) {
      case 1:
        _par_OrarioIntervalloHH.dalle_Limite_SX = "08:50:00";
        _par_OrarioIntervalloHH.dalle = "09:00:00";
        _par_OrarioIntervalloHH.dalle_Limite_DX = "09:10:00";

        _par_OrarioIntervalloHH.alle_Limite_SX = "13:00:00";
        _par_OrarioIntervalloHH.alle = "13:00:00";
        _par_OrarioIntervalloHH.alle_Limite_DX = "13:10:00";
        break;

      case 2:
        _par_OrarioIntervalloHH.dalle_Limite_SX = "13:50:00";
        _par_OrarioIntervalloHH.dalle = "14:00:00";
        _par_OrarioIntervalloHH.dalle_Limite_DX = "14:10:00";

        _par_OrarioIntervalloHH.alle_Limite_SX = "18:00:00";
        _par_OrarioIntervalloHH.alle = "18:00:00";
        _par_OrarioIntervalloHH.alle_Limite_DX = "18:10:00";
        break;

      case 3:
        _par_OrarioIntervalloHH.dalle_Limite_SX = "19:50:00";
        _par_OrarioIntervalloHH.dalle = "20:00:00";
        _par_OrarioIntervalloHH.dalle_Limite_DX = "20:10:00";

        _par_OrarioIntervalloHH.alle_Limite_SX = "21:00:00";
        _par_OrarioIntervalloHH.alle = "21:00:00";
        _par_OrarioIntervalloHH.alle_Limite_DX = "21:10:00";
        break;

      case 4:
        _par_OrarioIntervalloHH.dalle_Limite_SX = "21:50:00";
        _par_OrarioIntervalloHH.dalle = "22:00:00";
        _par_OrarioIntervalloHH.dalle_Limite_DX = "22:10:00";

        _par_OrarioIntervalloHH.alle_Limite_SX = "23:00:00";
        _par_OrarioIntervalloHH.alle = "23:00:00";
        _par_OrarioIntervalloHH.alle_Limite_DX = "23:10:00";
        break;
    }


    return _par_OrarioIntervalloHH;
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
