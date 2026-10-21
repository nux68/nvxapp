import { Component, ChangeDetectionStrategy } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError, of } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Az_ClienteGetInModel, Az_ClienteModel, Az_ClientePutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { AzClienteService } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/az-cliente.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ParTable } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-customer-edit-page',
  templateUrl: './customer-edit-page.component.html',
  styleUrls: ['./customer-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CustomerEditPageComponent extends BasePageConfirmCancelComponent<Az_ClienteModel> {

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private azClienteService: AzClienteService,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "Customer"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],

      partitaIva: [null, [Validators.pattern(/^\d{11}$/)]],
      codiceFiscale: [null, [Validators.pattern(/^([A-Za-z0-9]{16}|\d{11})$/)]],

      indirizzo: [null, [Validators.maxLength(100)]],
      cap: [null, [Validators.maxLength(10)]],
      citta: [null, [Validators.maxLength(50)]],
      provincia: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]],
      nazione: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]],

      telefono: [null, [Validators.maxLength(30)]],
      email: [null, [Validators.email, Validators.maxLength(100)]],
      pec: [null, [Validators.email, Validators.maxLength(100)]],
      referente: [null, [Validators.maxLength(100)]],

      codiceDestinatario: [null, [Validators.pattern(/^[A-Za-z0-9]{6,7}$/)]],

      note: [null, [Validators.maxLength(500)]]
    });
  }

  LoadData = (): Observable<Az_ClienteModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_ClienteGetInModel> = new GenericRequest<Az_ClienteGetInModel>(Az_ClienteGetInModel);
      request.data.id = state.id;
      return this.azClienteService.Az_ClienteGet(request).pipe(
        map((res) => res.data.az_Cliente),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Az_ClienteModel | null>((subscriber) => {
        subscriber.next(new Az_ClienteModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_ClienteModel): Observable<boolean> => {
    let request: GenericRequest<Az_ClientePutInModel> = new GenericRequest<Az_ClientePutInModel>(Az_ClientePutInModel);
    // codici in maiuscolo come da standard
    for (const campo of ['codiceFiscale', 'provincia', 'nazione', 'codiceDestinatario'] as const)
      if (editModel[campo]) editModel[campo] = editModel[campo].toUpperCase();
    request.data.az_Cliente = editModel;
    return this.azClienteService.Az_ClientePut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_Cliente);
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };
}

const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  return null;
};
