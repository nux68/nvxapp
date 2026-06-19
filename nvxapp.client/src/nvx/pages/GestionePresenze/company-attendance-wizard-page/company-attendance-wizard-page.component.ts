import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { CompanyEditModel, CompanyGetInModel, CompanyPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/company-model';

@Component({
  selector: 'app-company-attendance-wizard-page',
  templateUrl: './company-attendance-wizard-page.component.html',
  styleUrls: ['./company-attendance-wizard-page.component.scss'],
  standalone: false
})
export class CompanyAttendanceWizardPageComponent extends BasePageConfirmCancelComponent<CompanyEditModel> {

  /** 0 = Dati Aziendali, 1 = (seconda pagina vuota) */
  public currentStep = 0;
  public readonly totalSteps = 2;

  public modifiedDescription: string | null = null;

  /** Pulsanti di navigazione aggiuntivi. */
  private btnNext: ButtonItem;
  private btnBack: ButtonItem;

  /** Riferimenti ai pulsanti base (Conferma, Annulla) salvati dopo super(). */
  private baseConfirm: ButtonItem;
  private baseCancel: ButtonItem;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private authService: AuthService,
    private accountService: AccountService
  ) {
    super(navCtrl, userInterfaceService, fb);

    this.baseConfirm = this.buttonbar[0];
    this.baseCancel = this.buttonbar[1];

    this.btnNext = new ButtonItem('Avanti', 'arrow-forward-outline', 'primary', false, this.handleNext);
    this.btnBack = new ButtonItem('Indietro', 'arrow-back-outline', 'medium', false, this.handleBack);
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.updateButtonbar();
  }

  get Title(): string { return "Caricamento Dati Aziendali"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(20)]],
    });
  }

  LoadData = (): Observable<CompanyEditModel | null> => {
    this.currentStep = 0;
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<CompanyGetInModel> = new GenericRequest<CompanyGetInModel>(CompanyGetInModel);
      request.data.id = state.id;

      return this.accountService.CompanyGet(request).pipe(
        map((res) => res.data.companyEdit),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(new CompanyEditModel());
        })
      );
    } else {
      return of(new CompanyEditModel());
    }
  };

  SaveData = (editModel: CompanyEditModel): Observable<boolean> => {
    let request: GenericRequest<CompanyPutInModel> =
      new GenericRequest<CompanyPutInModel>(CompanyPutInModel);
    request.data.companyEdit = editModel;

    return this.accountService.CompanyPut(request).pipe(
      map(() => {
        this.authService.forceRolesEmission();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  UpdateDescription() {
    const descrizione = this._editForm.get('descrizione')?.value;

    if (descrizione) {
      this.modifiedDescription = "Attenzione per accedere a questa utenza verrano creati i seguenti user    ->   " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_Admin" + " / " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_PowerAdmin";
    } else {
      this.modifiedDescription = null;
    }
  }

  // ── Handler pulsanti ─────────────────────────────────────────────────────

  private handleNext = (param: object) => {
    if (this.currentStep === 0) {
      const step0Fields = ['descrizione'];
      this.validateFields(step0Fields);

      const step0Valid = step0Fields.every(f => this._editForm.get(f)?.valid);
      if (step0Valid) {
        this.currentStep = 1;
        this.updateButtonbar();
      }
    }
  };

  private handleBack = (param: object) => {
    if (this.currentStep > 0) {
      this.currentStep--;
      this.updateButtonbar();
    }
  };

  override ButtonConfirmClickEv = (param: object) => {
    if (this.currentStep === 1) {
      this.forceValidationAll();

      if (this._editForm.valid) {
        Object.assign(this._editModel, this._editForm.value);

        this.SaveData(this._editModel).subscribe(res => {
          if (res) {
            this.navCtrl.back();
          }
        });
      }
    }
  };

  // ── Gestione dinamica buttonbar ───────────────────────────────────────────

  private updateButtonbar(): void {
    if (this.currentStep === 0) {
      this.buttonbar = [this.btnNext];
    } else {
      this.baseConfirm.disabled = !this._editForm.valid;
      this.buttonbar = [this.btnBack, this.baseConfirm, this.baseCancel];
    }
  }

  // ── Validazione ───────────────────────────────────────────────────────────

  private validateFields(fieldNames: string[]): void {
    fieldNames.forEach(name => {
      const control = this._editForm.get(name);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });
  }

  private forceValidationAll(): void {
    Object.keys(this._editForm.controls).forEach(key => {
      const control = this._editForm.get(key);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });
  }
}