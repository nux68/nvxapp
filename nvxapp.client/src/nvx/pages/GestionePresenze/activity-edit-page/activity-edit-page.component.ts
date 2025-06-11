import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { Par_AttivitaModel, Par_AttivitaGetInModel, Par_AttivitaPutInModel, Par_AttivitaGetOutModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { ParAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Par_CompetenzaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CheckObjOn_Id_Number } from '../../../ClientServer-Service/ModelsBase/check-obj';

@Component({
  selector: 'app-activity-edit-page',
  templateUrl: './activity-edit-page.component.html',
  styleUrls: ['./activity-edit-page.component.scss'],
  standalone: false
})
export class ActivityEditPageComponent extends BasePageConfirmCancelComponent<Par_AttivitaGetOutModel> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private azAttivitaService: ParAttivitaService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  override ionViewWillEnter() {

    super.ionViewWillEnter();
    this._par_Competenza = this.sharedParameterGestionePresenzeService.Par_Competenza;
  }

  get Title(): string { return 'Attività'; }
  get EditForm(): FormGroup {
    return this.fb.group({

      //SUB FORM PER OGGETTi DI OGGETTI
      par_Attivita: this.fb.group({
        descrizione: [null, [Validators.required, Validators.maxLength(50)]],
        backgroundColor: [null, []],
        textColor: [null, []],
      })

      //descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      //backgroundColor: [null, []],
      //textColor: [null, []],
    });
  }

  LoadData = (): Observable<Par_AttivitaGetOutModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Par_AttivitaGetInModel> = new GenericRequest<Par_AttivitaGetInModel>(Par_AttivitaGetInModel);
      request.data.id = state.id;
      return this.azAttivitaService.Par_AttivitaGet(request).pipe(
        map((res) => {
          return res.data;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Par_AttivitaGetOutModel | null>((subscriber) => {
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();
        subscriber.next(new Par_AttivitaGetOutModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_AttivitaGetOutModel): Observable<boolean> => {
    let request: GenericRequest<Par_AttivitaPutInModel> =
      new GenericRequest<Par_AttivitaPutInModel>(Par_AttivitaPutInModel);

    

    request.data.par_Attivita = editModel.par_Attivita;
    request.data.par_Competenza = editModel.par_Competenza;
    return this.azAttivitaService.Par_AttivitaPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };

  public searchText!: string;
  public currSection: string = "sez1";
  public _par_Competenza: Par_CompetenzaModel[] = [];

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

  public getPar_Competenza(): CheckObjOn_Id_Number[] {

    let retVal: CheckObjOn_Id_Number[] = [];

    this._par_Competenza.filter(rep =>
      rep.id > 0
    ).forEach(item => {
      let appo = { id: item.id, checked: false };
      retVal.push(appo);
    });


    return retVal;
  }

  isSelectedaz_Par_AttivitaCompetenza(itemId: number): boolean {


    return this._editModel.par_Competenza.find(entry => entry.id === itemId)?.checked ?? false;

  }

  toggleSelectionaz_Par_AttivitaCompetenza(itemId: number, event: any) {



    const existingEntry = this._editModel.par_Competenza.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.par_Competenza.push({ id: itemId, checked: event.detail.checked });
    }

  }

  public getAttivitaDescr(): string {

    if (this?._editModel?.par_Attivita?.descrizione) {
      return  ' di : "'+ this._editModel.par_Attivita.descrizione+ '"';
    }
    else
      return '';

  }

}

const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  return null;
};
