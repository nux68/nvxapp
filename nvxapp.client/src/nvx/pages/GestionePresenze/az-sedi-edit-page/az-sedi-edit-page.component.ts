import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { Az_SediModel, Az_SediGetInModel, Az_SediPutInModel, Az_SediGetOutModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { CheckObjOn_Id_Number } from '../../../ClientServer-Service/ModelsBase/check-obj';

@Component({
  selector: 'app-az-sedi-edit-page',
  templateUrl: './az-sedi-edit-page.component.html',
  styleUrls: ['./az-sedi-edit-page.component.scss'],
  standalone: false
})
export class AzSediEditPageComponent extends BasePageConfirmCancelComponent<Az_SediGetOutModel> {

  public searchText!: string;
  public currSection: string = "sez1";
  public _par_Attivita: Par_AttivitaModel[] = [];

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private azSediService: AzSediService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  override ionViewWillEnter() {

    super.ionViewWillEnter();
    this._par_Attivita = this.sharedParameterGestionePresenzeService.Par_Attivita;
  }

  get Title(): string { return 'Sede'; }
  get EditForm(): FormGroup {
    return this.fb.group({
      //SUB FORM PER OGGETTi DI OGGETTI
      az_Sedi: this.fb.group({
        descrizione: [null, [Validators.required, Validators.maxLength(20)]],
      })
    });
  }

  LoadData = (): Observable<Az_SediGetOutModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_SediGetInModel> = new GenericRequest<Az_SediGetInModel>(Az_SediGetInModel);
      request.data.id = state.id;
      return this.azSediService.AzSediGet(request).pipe(
        map((res) => {
          return res.data;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Az_SediGetOutModel | null>((subscriber) => {
        subscriber.next(new Az_SediGetOutModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_SediGetOutModel): Observable<boolean> => {
    let request: GenericRequest<Az_SediPutInModel> =
      new GenericRequest<Az_SediPutInModel>(Az_SediPutInModel);
    request.data.az_Sedi = editModel.az_Sedi;
    request.data.az_SediAttivita = editModel.az_SediAttivita;

    return this.azSediService.AzSediPut(request).pipe(
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


  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }


  public getPar_Attivita(): CheckObjOn_Id_Number[] {

    let retVal: CheckObjOn_Id_Number[] = [];

    this._par_Attivita.filter(rep =>
      rep.id > 0
    ).forEach(item => {
      let appo = { id: item.id, checked: false };
      retVal.push(appo);
    });


    return retVal;
  }

  isSelectedaz_Az_SediRepartoAttivita(itemId: number): boolean {



    return this._editModel.az_SediAttivita.find(entry => entry.id === itemId)?.checked ?? false;

  }

  toggleSelectionaz_Az_SediRepartoAttivita(itemId: number, event: any) {



    const existingEntry = this._editModel.az_SediAttivita.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SediAttivita.push({ id: itemId, checked: event.detail.checked });
    }

  }

  public getSedeDescr(): string {

    if (this?._editModel?.az_Sedi?.descrizione) {
      return ' di : "' + this._editModel.az_Sedi.descrizione + '"';
    }
    else
      return '';

  }

}
