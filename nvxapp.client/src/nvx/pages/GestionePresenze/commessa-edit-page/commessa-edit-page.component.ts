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
import { Az_CommessaModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { AzCommessaService, Az_CommessaGetInModel, Az_CommessaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

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
      idAz_Cliente: [null, [Validators.required]]
    });
  }

  override get EditForm(): FormGroup {
    return this._editForm;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this._az_ClienteModelList = this.sharedParameterGestionePresenzeService.Az_Cliente;
  }

  get Title(): string { return 'Commessa'; }

  LoadData = (): Observable<Az_CommessaModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_CommessaGetInModel> = new GenericRequest<Az_CommessaGetInModel>(Az_CommessaGetInModel);
      request.data.id = state.id;
      return this.azCommessaService.AZ_CommessaGet(request).pipe(
        map((res) => res.data.az_Commessa), // Estrae il dato richiesto
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
    return this.azCommessaService.AZ_CommessaPut(request).pipe(
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
}
