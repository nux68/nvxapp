import { ChangeDetectorRef, Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subscription } from 'rxjs';
import { map, catchError, filter } from 'rxjs/operators';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Dip_Anagrafica4EditModel, Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Put_InModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DipAnagraficaService } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { Dip_RapportoLavoroModel } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { Dip_ProfiloOrarioModel } from '../../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { DbUtilService } from '../../../Utility/infrastructure/db-util.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-user-department-wizard-page',
  templateUrl: './user-department-wizard-page.component.html',
  styleUrls: ['./user-department-wizard-page.component.scss'],
  standalone: false
})
export class UserDepartmentWizardPageComponent extends BasePageConfirmCancelComponent<Dip_Anagrafica4EditModel> {

  /** 0 = Anagrafica, 1 = Profilo Orario (ultimo step) */
  public currentStep = 0;
  public readonly totalSteps = 2;

  /** Profilo orario selezionato nello step 1 (idPar_ProfiloOrario). */
  public selectedProfiloOrarioId: number | null = null;

  /** Date di validità del profilo orario (legate al modello corrente). */
  public profiloDal: string | null = null;
  public profiloAl: string | null = null;

  /** true quando i dati sono stati caricati dal server (username readonly). */
  public isFromServer = false;

  /** Pulsanti di navigazione aggiuntivi. */
  private btnNext: ButtonItem;
  private btnBack: ButtonItem;

  /** Riferimenti ai pulsanti base (Conferma, Annulla) salvati dopo super(). */
  private baseConfirm: ButtonItem;
  private baseCancel: ButtonItem;

  /** Subscription per reagire al caricamento asincrono di Par_ProfiloOrario. */
  private profiloOrarioSub: Subscription | null = null;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private dipAnagraficaService: DipAnagraficaService,
    private dbUtilService: DbUtilService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private parameterService: ParameterService,
    private cdr: ChangeDetectorRef
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

    // Ascolta il caricamento asincrono dei profili orario.
    // Se il modello è già caricato ma manca il profilo (dati non ancora disponibili),
    // lo popola appena i parametri arrivano.
    const profili = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario;
    if (profili && profili.length > 0 && this._editModel) {
      this.ensureProfiloOrarioExists();
    } else {
      this.profiloOrarioSub = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario$
        .pipe(filter(p => p != null && p.length > 0))
        .subscribe(() => {
          if (this._editModel) {
            this.ensureProfiloOrarioExists();
          }
        });
    }
  }

  ionViewWillLeave() {
    this.profiloOrarioSub?.unsubscribe();
    this.profiloOrarioSub = null;
  }

  get Title(): string { return "Nuovo Utente"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      cognome: [null, [Validators.required, Validators.maxLength(50)]],
      nome: [null, [Validators.required, Validators.maxLength(50)]],
      mail: [null, [Validators.required, Validators.email]],
      roles: [[], [this.minArrayLength(1)]],
    });
  }

  minArrayLength(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      return control.value && control.value.length >= min ? null : { minArrayLength: true };
    };
  }

  LoadData = (): Observable<Dip_Anagrafica4EditModel | null> => {
    this._editModel = null;
    this.currentStep = 0;
    this.isFromServer = false;

    const state = history.state;

    // Carica i dati dal server se arriva un idAspNetUsers
    if (state && state.id) {
      const request = new GenericRequest<Dip_Anagrafica_Get_InModel>(Dip_Anagrafica_Get_InModel);
      request.data.id = state.id;

      return this.dipAnagraficaService.Dip_AnagraficaGet(request).pipe(
        map((res) => {
          this.isFromServer = true;
          const dip_Anagrafica = res.data.dip_Anagrafica;
          // Assegna subito _editModel così ensureProfiloOrarioExists può leggerlo
          this._editModel = dip_Anagrafica;
          this.syncProfiloFromModel(dip_Anagrafica);
          // Popola il profilo di default se i parametri sono già caricati;
          // se non lo sono, ci penserà la subscription in ionViewWillEnter.
          this.ensureProfiloOrarioExists();
          return dip_Anagrafica;
        }),
        catchError((error): Observable<Dip_Anagrafica4EditModel | null> => {
          console.error('Errore durante il caricamento dati utente:', error);
          return of(this.buildNewModel());
        })
      );
    }

    return of(this.buildNewModel());
  };

  /** Restituisce l'ultimo giorno dell'anno in formato ISO (YYYY-MM-DD). */
  private getUltimoGiornoAnno(): string {
    const anno = new Date().getFullYear();
    return `${anno}-12-31T00:00:00`;
  }

  /** Costruisce un modello nuovo (fallback / nessun id). */
  private buildNewModel(): Dip_Anagrafica4EditModel {
    const dip_Anagrafica4Edit = new Dip_Anagrafica4EditModel();
    dip_Anagrafica4Edit.dip_RapportoLavoro = [];
    dip_Anagrafica4Edit.id = this.dbUtilService.GenerateCounterKey();

    const dip_RapportoLavoro = new Dip_RapportoLavoroModel();
    dip_RapportoLavoro.idDip_Anagrafica = dip_Anagrafica4Edit.id;
    dip_RapportoLavoro.id = this.dbUtilService.GenerateCounterKey();
    dip_Anagrafica4Edit.dip_RapportoLavoro.push(dip_RapportoLavoro);

    if (this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.length > 0) {
      const defaultProfilo = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario[0];
      const dip_ProfiloOrario = new Dip_ProfiloOrarioModel();
      dip_ProfiloOrario.id = this.dbUtilService.GenerateCounterKey();
      dip_ProfiloOrario.idDip_RapportoLavoro = dip_RapportoLavoro.id;
      dip_ProfiloOrario.numGiornoPartenzaCiclo = 1;
      dip_ProfiloOrario.idPar_ProfiloOrario = defaultProfilo.id;
      dip_ProfiloOrario.dal = new Date().toISOString();
      dip_ProfiloOrario.al = this.getUltimoGiornoAnno();
      dip_Anagrafica4Edit.dip_ProfiloOrario.push(dip_ProfiloOrario);
    }

    this.syncProfiloFromModel(dip_Anagrafica4Edit);
    return dip_Anagrafica4Edit;
  }

  SaveData = (editModel: Dip_Anagrafica4EditModel): Observable<boolean> => {
    const request = new GenericRequest<Dip_Anagrafica_Put_InModel>(Dip_Anagrafica_Put_InModel);
    request.data.dip_Anagrafica = editModel;
    request.data.id = editModel.idAspNetUsers;
    return this.dipAnagraficaService.Dip_AnagraficaPut(request).pipe(
      map(() => true),
      catchError((error): Observable<boolean> => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  /** Se dip_ProfiloOrario è vuoto ma i parametri sono disponibili, crea il profilo di default. */
  private ensureProfiloOrarioExists(): void {
    if (!this._editModel) return;
    if (this._editModel.dip_ProfiloOrario.length > 0) return;

    const profili = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario;
    if (!profili || profili.length === 0) return;

    const rapportoId = this._editModel.dip_RapportoLavoro?.[0]?.id ?? 0;
    const defaultProfilo = profili[0];
    const nuovoProfilo = new Dip_ProfiloOrarioModel();
    nuovoProfilo.id = this.dbUtilService.GenerateCounterKey();
    nuovoProfilo.idDip_RapportoLavoro = rapportoId;
    nuovoProfilo.numGiornoPartenzaCiclo = 1;
    nuovoProfilo.idPar_ProfiloOrario = defaultProfilo.id;
    nuovoProfilo.dal = new Date().toISOString();
    nuovoProfilo.al = this.getUltimoGiornoAnno();
    this._editModel.dip_ProfiloOrario.push(nuovoProfilo);

    this.syncProfiloFromModel(this._editModel);
    this.cdr.detectChanges();
  }

  // ── Sincronizzazione profilo ──────────────────────────────────────────────

  private syncProfiloFromModel(model: Dip_Anagrafica4EditModel): void {
    if (model.dip_ProfiloOrario && model.dip_ProfiloOrario.length > 0) {
      const profilo = model.dip_ProfiloOrario[0];
      this.selectedProfiloOrarioId = profilo.idPar_ProfiloOrario;
      this.profiloDal = this.formatDateForDatetime(profilo.dal) ?? this.formatDateForDatetime(new Date().toISOString());
      this.profiloAl = this.formatDateForDatetime(profilo.al) ?? this.formatDateForDatetime(new Date().toISOString());
    } else {
      this.selectedProfiloOrarioId = null;
      this.profiloDal = null;
      this.profiloAl = null;
    }
  }

  private syncProfiloToModel(): void {
    if (!this._editModel) return;

    // Se l'utente ha selezionato un profilo ma l'array è vuoto, crea il record
    if (this.selectedProfiloOrarioId != null && this._editModel.dip_ProfiloOrario.length === 0) {
      const rapportoId = this._editModel.dip_RapportoLavoro?.[0]?.id ?? 0;
      const nuovoProfilo = new Dip_ProfiloOrarioModel();
      nuovoProfilo.id = this.dbUtilService.GenerateCounterKey();
      nuovoProfilo.idDip_RapportoLavoro = rapportoId;
      nuovoProfilo.numGiornoPartenzaCiclo = 1;
      nuovoProfilo.idPar_ProfiloOrario = this.selectedProfiloOrarioId;
      nuovoProfilo.dal = this.profiloDal ?? new Date().toISOString();
      nuovoProfilo.al = this.profiloAl ?? this.getUltimoGiornoAnno();
      this._editModel.dip_ProfiloOrario.push(nuovoProfilo);
      return;
    }

    if (this._editModel.dip_ProfiloOrario.length > 0) {
      const profilo = this._editModel.dip_ProfiloOrario[0];

      // Se il profilo non è mai stato salvato su server, sincronizza il riferimento
      if (profilo.idPar_ProfiloOrario === 0 && this.selectedProfiloOrarioId != null) {
        profilo.idPar_ProfiloOrario = this.selectedProfiloOrarioId;
      }

      if (this.selectedProfiloOrarioId != null) {
        profilo.idPar_ProfiloOrario = this.selectedProfiloOrarioId;
      }
      if (this.profiloDal != null) {
        profilo.dal = this.profiloDal;
      }
      if (this.profiloAl != null) {
        profilo.al = this.profiloAl;
      }
    }
  }

  // ── Handler pulsanti ─────────────────────────────────────────────────────

  private handleNext = (param: object) => {
    if (this.currentStep === 0) {
      const step1Fields = ['descrizione', 'cognome', 'nome', 'mail', 'roles'];
      this.validateFields(step1Fields);

      const step1Valid = step1Fields.every(f => this._editForm.get(f)?.valid);
      if (step1Valid) {
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

      if (this._editForm.valid && this.selectedProfiloOrarioId != null) {
        this.syncProfiloToModel();
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
      this.baseConfirm.disabled = !this._editForm.valid || this.selectedProfiloOrarioId == null;
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

  // ── Eventi template ───────────────────────────────────────────────────────

  onProfiloOrarioChange(): void {
    this.updateButtonbar();
  }

  onDalChange(event: any): void {
    this.profiloDal = event.detail.value;
    this.syncProfiloToModel();
  }

  onAlChange(event: any): void {
    this.profiloAl = event.detail.value;
    this.syncProfiloToModel();
  }

  /** Formatta una data ISO per l'attributo value di ion-datetime. */
  formatDateForDatetime(iso: string | null): string | null {
    if (!iso) return null;
    return iso.substring(0, 10); // YYYY-MM-DD
  }

  getRoles(): string[] {
    if (this._editModel && this._editModel.idUserCompany === 0) {
      return this.parameterService.Roles
        .filter(role => role.code === RoleCode.User)
        .map(role => role.name);
    }
    return this.parameterService.Roles.map(role => role.name);
  }

  get profiliOrario() {
    return this.sharedParameterGestionePresenzeService.Par_ProfiloOrario;
  }

  /** Necessario a ion-select per confrontare l'opzione col valore selezionato. */
  compareProfiloOrario = (o1: any, o2: any): boolean => {
    const v1 = typeof o1 === 'number' ? o1 : (o1?.id ?? o1);
    const v2 = typeof o2 === 'number' ? o2 : (o2?.id ?? o2);
    return v1 === v2;
  };
}