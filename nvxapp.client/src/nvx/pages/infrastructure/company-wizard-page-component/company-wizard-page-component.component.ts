import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { map, catchError, Observable } from 'rxjs';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { TranslocoService } from '@jsverse/transloco';

import { CompanyEditModel, CompanyGetInModel, CompanyPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/company-model';

@Component({
  selector: 'app-company-wizard-page-component',
  templateUrl: './company-wizard-page-component.component.html',
  styleUrls: ['./company-wizard-page-component.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyWizardPageComponentComponent  extends BasePageConfirmCancelComponent<CompanyEditModel> {

  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private authService: AuthService,
    private accountService: AccountService,
    private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

  }

  get Title(): string { return this.transloco.translate('pages.companyWizard.title'); }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(20)]],

    });
  }

  LoadData = (): Observable<CompanyEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<CompanyGetInModel> = new GenericRequest<CompanyGetInModel>(CompanyGetInModel);
      request.data.id = state.id;

      return this.accountService.CompanyGet(request).pipe(
        map((res) => res.data.companyEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null as any]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<CompanyEditModel | null>((subscriber) => {
        subscriber.next(new CompanyEditModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: CompanyEditModel): Observable<boolean> => {
    let request: GenericRequest<CompanyPutInModel> =
      new GenericRequest<CompanyPutInModel>(CompanyPutInModel);
    request.data.companyEdit = editModel;

    return this.accountService.CompanyPut(request).pipe(
      map(() => {
        this.authService.forceRolesEmission(); // Forza l'emissione dei ruoli per aggiornare i parametri
        return true;
      }), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  UpdateDescription() {
    const descrizione = this._editForm.get('descrizione')?.value;

    if (descrizione) {
      const userBase = this.stringHelperService.removeSpecialCharacters(descrizione);
      this.modifiedDescription = this.transloco.translate('pages.companyWizard.usersWarning', { admin: userBase + "_Admin", powerAdmin: userBase + "_PowerAdmin" });
    } else {
      this.modifiedDescription = null;
    }
  }

}