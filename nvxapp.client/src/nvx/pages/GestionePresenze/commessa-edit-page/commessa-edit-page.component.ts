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
import { Az_CommessaGetInModel, Az_CommessaModel, Az_CommessaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';

@Component({
  selector: 'app-commessa-edit-page',
  templateUrl: './commessa-edit-page.component.html',
  styleUrls: ['./commessa-edit-page.component.scss'],
  standalone: false
})
export class CommessaEditPageComponent extends BasePageConfirmCancelComponent<Az_CommessaModel> {

  public override _editForm: FormGroup;
  public _az_ClienteModelList: Az_ClienteModel[] = [];



  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private azCommessaService: AzCommessaService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
    this._editForm = this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      idAz_Cliente: [null, [Validators.required]],
      //data: [null, [Validators.required]],
      //dataA: [null, [Validators.required]]
    });
  }

  override get EditForm(): FormGroup {
    return this._editForm;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this._az_ClienteModelList = this.sharedParameterGestionePresenzeService.Az_Cliente;

    const now = new Date();

    // Set time to midnight for consistency when dealing with dates only
    now.setHours(0, 0, 0, 0);
    this.startDate = this.stringHelperService.DateCurr_To_ISOString();
    this.endDate = this.stringHelperService.DateCurr_To_ISOString();
    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);

  }

  get Title(): string { return 'Commessa'; }

  LoadData = (): Observable<Az_CommessaModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_CommessaGetInModel> = new GenericRequest<Az_CommessaGetInModel>(Az_CommessaGetInModel);
      request.data.id = state.id;
      return this.azCommessaService.Az_CommessaGet(request).pipe(
        map((res) => {

          this.startDate = res.data.az_Commessa.data; //this.stringHelperService.DateCurr_To_ISOString();
          this.endDate = res.data.az_Commessa.dataA; //this.stringHelperService.DateCurr_To_ISOString();

          this.formattedStartDate = res.data.az_Commessa.data; //this.stringHelperService.Date_To_S_ddmmyyyy(now);
          this.formattedEndDate = res.data.az_Commessa.dataA; //this.stringHelperService.Date_To_S_ddmmyyyy(now);

          return res.data.az_Commessa;

        }), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    } else {
      return new Observable<Az_CommessaModel | null>((subscriber) => {
        subscriber.next(new Az_CommessaModel());
        subscriber.complete();
      });
    }
  };

  public SaveData(editModel: Az_CommessaModel): Observable<boolean> {
    let request: GenericRequest<Az_CommessaPutInModel> = new GenericRequest<Az_CommessaPutInModel>(Az_CommessaPutInModel);
    request.data.az_Commessa = editModel;

    
    request.data.az_Commessa.data = this.formattedStartDate;
    request.data.az_Commessa.dataA = this.formattedEndDate;


    return this.azCommessaService.Az_CommessaPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  }


  public startDate: string;
  public formattedStartDate: string;
  public endDate: string;
  public formattedEndDate: string;

  updateStartDate(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    //// Controlla se la data di inizio è successiva alla data di fine
    //if (this.compareDates(this.formattedStartDate, this.formattedEndDate) > 0) {
    //  // Aggiorna la data di fine per farla coincidere con la data di inizio
    //  this.endDate = event.detail.value;
    //  this.formattedEndDate = this.formattedStartDate;
    //}
  }

  updateEndDate(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    //// Controlla se la data di fine è precedente alla data di inizio
    //if (this.compareDates(this.formattedEndDate, this.formattedStartDate) < 0) {
    //  // Aggiorna la data di inizio per farla coincidere con la data di fine
    //  this.startDate = event.detail.value;
    //  this.formattedStartDate = this.formattedEndDate;
    //}
  }


}
