import { Component, ChangeDetectionStrategy, ElementRef, OnInit } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonInput, IonItem, IonLabel, IonModal, IonNote, IonProgressBar, IonRow, IonSelect, IonSelectOption, IonSpinner, IonText, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { RefresherService } from '../../../../Utility/GestionePresenze/refresher.service';
import { firstValueFrom, from, Observable, of } from 'rxjs';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { UserCheckInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-check-model';
import { DestinazioneEccedenze, TipoContratto } from '../../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { Par_GiustificativiModel, TipoContatore } from '../../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { Par_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { Par_CompetenzaModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { SharedParameterGestionePresenzeService, ParTable } from '../../../shared-parameter-gestione-presenze.service';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { NuovoDipendenteService } from '../../../../Utility/GestionePresenze/nuovo-dipendente.service';
import { codiceFiscaleValidator } from '../../../../Utility/GestionePresenze/codice-fiscale';
import { DipAnagraficaService } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { Dip_Anagrafica_Check_InModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';

/** valori del wizard: un gruppo del form per gli step 1-3; dopo il salvataggio l'utente creato */
export class UserAttendanceWizardModel {
  utente: any = null;
  rapporto: any = null;
  profilo: any = null;
  idAspNetUsers: string | null = null;
}

/**
 * Inserimento di un nuovo dipendente a step:
 *  1 utente (cognome, nome, username, password, email, ruoli)
 *  2 rapporto di lavoro (senza data di licenziamento)
 *  3 profilo orario
 *  4 competenze
 *  5 maturazione mensile dei contatori
 * L'utente viene creato solo alla conferma dell'ultimo step: annullando non resta nulla di salvato.
 * Sostituisce il dialog di inserimento utente di Infrastructure (vedi MainMenuAttendanceTrackingService.RedefineDialog):
 * e' standalone, cosi' la pagina di Infrastructure che lo apre non deve importare moduli della gestione presenze.
 * Form (_editForm della classe base): gruppi "utente", "rapporto" e "profilo", uno per step; si valida solo quello corrente.
 */
@Component({
  selector: 'app-user-attendance-wizard-dialog',
  templateUrl: './user-attendance-wizard-dialog.component.html',
  styleUrls: ['./user-attendance-wizard-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonInput, IonItem, IonLabel, IonModal, IonNote, IonProgressBar, IonRow, IonSelect, IonSelectOption, IonSpinner, IonText, IonTitle, IonToolbar, SharedComponentInfrastructureModule]
})
export class UserAttendanceWizardDialogComponent extends BaseDialogConfirmCancelComponent<UserAttendanceWizardModel> implements OnInit {

  public readonly titoliStep: string[];
  public readonly tipoContrattoEnum = TipoContratto;
  public readonly destinazioneEccedenzeEnum = DestinazioneEccedenze;

  public currentStep: number = 0;
  public saving: boolean = false;
  /** verifica in corso di username ed email (passaggio dallo step 1 allo step 2) */
  public checking: boolean = false;
  public erroreSalvataggio: string | null = null;

  private btnNext: ButtonItem;
  private btnBack: ButtonItem;

  // un gruppo del form per step: si valida solo quello corrente
  public get formUtente(): FormGroup { return this._editForm?.get('utente') as FormGroup; }
  public get formRapporto(): FormGroup { return this._editForm?.get('rapporto') as FormGroup; }
  public get formProfilo(): FormGroup { return this._editForm?.get('profilo') as FormGroup; }

  /** Competenze selezionate (step 4). */
  public competenzeSelezionate: number[] = [];
  /** Ore di maturazione mensile per giustificativo-contatore (step 5), formato "HHH:MM". */
  public maturazioni: { [idPar_Giustificativi: number]: string } = {};

  public par_ProfiloOrario: Par_ProfiloOrarioModel[] = [];
  public par_Competenza: Par_CompetenzaModel[] = [];
  public giustificativiContatore: Par_GiustificativiModel[] = [];

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private parameterService: ParameterService,
              private accountService: AccountService,
              private nuovoDipendenteService: NuovoDipendenteService,
              private dipAnagraficaService: DipAnagraficaService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private refresherService: RefresherService,
              private elementRef: ElementRef<HTMLElement>,
              private transloco: TranslocoService) {

    super(userInterfaceService, fb, modalCtrl);

    this.titoliStep = ['utente', 'rapporto', 'profilo', 'competenze', 'maturazione'].map(s => this.t('steps.' + s));

    this.btnNext = new ButtonItem(this.t('avanti'), 'arrow-forward-outline', 'primary', false, this.handleNext);
    this.btnBack = new ButtonItem(this.t('indietro'), 'arrow-back-outline', 'medium', false, this.handleBack);
    // conferma: salvataggio in piu' chiamate (utente, poi dati presenze); annulla: quello della classe base
    this.btnConfirm.event = this.handleConfirm;
  }

  get EditForm(): FormGroup {
    const userRole = this.parameterService.Roles.find(role => role.code === RoleCode.User);
    this.par_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario ?? [];
    const proposti = this.nuovoDipendenteService.Default();

    return this.fb.group({
      utente: this.fb.group({
        cognome: [null, [Validators.required, Validators.maxLength(50)]],
        nome: [null, [Validators.required, Validators.maxLength(50)]],
        matricola: [null, [Validators.maxLength(20)]],
        codiceFiscale: [null, [codiceFiscaleValidator()]],
        descrizione: [null, [Validators.required, Validators.maxLength(50)]],
        pw: [null, [Validators.required]],
        mail: [null, [Validators.required, Validators.email]],
        roles: [userRole ? [userRole.name] : [], [minArrayLength(1)]],
      }),
      rapporto: this.fb.group({
        dataAss: [proposti.rapporto.dataAss, [Validators.required]],
        tipoContratto: [proposti.rapporto.tipoContratto],
        oreSettimanaliContratto: [proposti.rapporto.oreSettimanaliContratto, [Validators.min(0), Validators.max(168)]],
        ccnl: [proposti.rapporto.ccnl, [Validators.maxLength(50)]],
        destinazioneEccedenze: [proposti.rapporto.destinazioneEccedenze],
      }),
      profilo: this.fb.group({
        idPar_ProfiloOrario: [proposti.profilo.idPar_ProfiloOrario, [Validators.required]],
        numGiornoPartenzaCiclo: [proposti.profilo.numGiornoPartenzaCiclo, [Validators.required, Validators.min(1)]],
      }),
    });
  }

  override ngOnInit() {
    super.ngOnInit();
    this.ingrandisciDialog();

    this.par_Competenza = this.sharedParameterGestionePresenzeService.Par_Competenza ?? [];
    this.competenzeSelezionate = this.nuovoDipendenteService.Default().competenze;

    this.giustificativiContatore = (this.sharedParameterGestionePresenzeService.Par_Giustificativi ?? [])
      .filter(j => j.tipoContatore !== TipoContatore.NoContatore);
    this.giustificativiContatore.forEach(j => this.maturazioni[j.id] = '00:00');

    this.updateButtonbar();
  }

  get Title(): string { return this.t('title'); }

  private t(key: string): string {
    return this.transloco.translate('attendance.components.userAttendanceWizardDialog.' + key);
  }

  // all'apertura il componente e' gia' dentro ion-modal (in ngOnInit puo' non esserlo ancora)
  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.ingrandisciDialog();
  }

  // nuovo dipendente: i valori sono quelli proposti dal form
  LoadData = (): Observable<UserAttendanceWizardModel> => {
    return of(Object.assign(new UserAttendanceWizardModel(), this._editForm.value));
  };

  /** crea l'utente e salva i dati delle presenze; restituisce il modello con l'utente creato (null se non riuscito) */
  SaveData = (editModel: UserAttendanceWizardModel): Observable<UserAttendanceWizardModel | null> => {
    return from(this.salva(editModel));
  };

  /** Dialog piu' grande del normale da tablet in su (su mobile resta a tutto schermo). */
  private ingrandisciDialog(): void {
    const modal = this.elementRef.nativeElement.closest('ion-modal');
    if (modal && window.matchMedia('(min-width: 576px)').matches) {
      modal.style.setProperty('--width', 'min(900px, 92vw)');
      modal.style.setProperty('--height', 'min(780px, 90vh)');
    }
  }

  // ── Ruoli (come nell'inserimento utente) ─────────────────────────────────

  getRoles(): string[] {
    const rolesPowerAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyPowerAdmin);
    const selezionati: string[] = this.formUtente?.get('roles')?.value ?? [];
    if (rolesPowerAdmin && selezionati.includes(rolesPowerAdmin.name)) {
      return this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyPowerAdmin).map(role => role.name);
    }
    return this.parameterService.Roles.filter(role => role.code == RoleCode.User || role.code == RoleCode.CompanyAdmin)
      .map(role => role.name);
  }

  // ── Step 2: data di assunzione ───────────────────────────────────────────

  /** Salva solo il giorno scelto: senza ora e fuso orario il server non lo sposta al giorno prima. */
  onDataAssChange(event: any): void {
    const value = event?.detail?.value;
    if (typeof value !== 'string' || value.length < 10) return;
    this.formRapporto.get('dataAss')?.setValue(value.substring(0, 10) + 'T00:00:00');
  }

  // ── Step 4: competenze ───────────────────────────────────────────────────

  isCompetenzaSelezionata(id: number): boolean {
    return this.competenzeSelezionate.includes(id);
  }

  toggleCompetenza(id: number, event: any): void {
    if (event.detail.checked) {
      if (!this.competenzeSelezionate.includes(id)) this.competenzeSelezionate.push(id);
    } else {
      this.competenzeSelezionate = this.competenzeSelezionate.filter(x => x !== id);
    }
  }

  // ── Step 5: maturazioni ──────────────────────────────────────────────────

  isOreValide(valore: string | null | undefined): boolean {
    return !valore || /^\d+:[0-5]\d$/.test(valore);
  }

  private get maturazioniValide(): boolean {
    return this.giustificativiContatore.every(j => this.isOreValide(this.maturazioni[j.id]));
  }

  // ── Navigazione ──────────────────────────────────────────────────────────

  private formDelloStep(step: number): FormGroup | null {
    switch (step) {
      case 0: return this.formUtente;
      case 1: return this.formRapporto;
      case 2: return this.formProfilo;
      default: return null;
    }
  }

  private stepValido(step: number): boolean {
    const form = this.formDelloStep(step);
    if (form) {
      form.markAllAsTouched();
      form.updateValueAndValidity();
      return form.valid;
    }
    if (step === 4) return this.maturazioniValide;
    return true;
  }

  private handleNext = async () => {
    if (this.checking || !this.stepValido(this.currentStep)) return;
    // dallo step 1: username ed email non devono essere già usati da un altro utente, matricola e codice fiscale da un altro dipendente
    if (this.currentStep === 0 && !(await this.verificaUsernameMail())) return;
    if (this.currentStep === 0 && !(await this.verificaMatricolaCodiceFiscale())) return;
    if (this.currentStep < this.titoliStep.length - 1) {
      this.currentStep++;
      this.updateButtonbar();
    }
  };

  /**
   * Controlla in archivio username ed email: se già usati segna l'errore sul campo e resta allo step 1.
   * In caso di errore della chiamata non blocca: il controllo definitivo resta alla creazione dell'utente.
   */
  private async verificaUsernameMail(): Promise<boolean> {
    const request = new GenericRequest<UserCheckInModel>(UserCheckInModel);
    request.data.userName = this.formUtente.value.descrizione;
    request.data.mail = this.formUtente.value.mail;

    this.checking = true;
    this.updateButtonbar();
    try {
      const res = await firstValueFrom(this.accountService.UserCheck(request));
      if (!res?.success || !res.data) return true;
      this.segnaGiaUsato('descrizione', res.data.userNameExists);
      this.segnaGiaUsato('mail', res.data.mailExists);
      return !res.data.userNameExists && !res.data.mailExists;
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      return true;
    } finally {
      this.checking = false;
      this.updateButtonbar();
    }
  }

  /**
   * Matricola e codice fiscale (se indicati) non devono essere già di un altro dipendente dell'azienda:
   * l'utente si crea prima dei dati delle presenze, quindi il controllo va fatto qui.
   * In caso di errore della chiamata non blocca: resta il controllo al salvataggio.
   */
  private async verificaMatricolaCodiceFiscale(): Promise<boolean> {
    const matricola = (this.formUtente.value.matricola ?? '').trim();
    const codiceFiscale = (this.formUtente.value.codiceFiscale ?? '').trim();
    this.segnaGiaUsato('matricola', false);
    this.segnaGiaUsato('codiceFiscale', false);
    if (!matricola && !codiceFiscale) return true;

    const request = new GenericRequest<Dip_Anagrafica_Check_InModel>(Dip_Anagrafica_Check_InModel);
    request.data.matricola = matricola || null;
    request.data.codiceFiscale = codiceFiscale || null;

    this.checking = true;
    this.updateButtonbar();
    try {
      const res = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaCheck(request));
      if (!res?.success || !res.data) return true;
      this.segnaGiaUsato('matricola', res.data.matricolaExists);
      this.segnaGiaUsato('codiceFiscale', res.data.codiceFiscaleExists);
      return !res.data.matricolaExists && !res.data.codiceFiscaleExists;
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      return true;
    } finally {
      this.checking = false;
      this.updateButtonbar();
    }
  }

  /** Aggiunge o toglie l'errore "giaUsato" lasciando gli altri errori del campo. */
  private segnaGiaUsato(campo: string, giaUsato: boolean): void {
    const control = this.formUtente.get(campo);
    if (!control) return;
    const { giaUsato: _, ...altri } = control.errors ?? {};
    const errori = giaUsato ? { ...altri, giaUsato: true } : altri;
    control.setErrors(Object.keys(errori).length ? errori : null);
    control.markAsTouched();
  }

  private handleBack = () => {
    if (this.currentStep > 0) {
      this.currentStep--;
      this.updateButtonbar();
    }
  };

  private updateButtonbar(): void {
    const ultimo = this.currentStep === this.titoliStep.length - 1;
    this.btnConfirm.disabled = this.saving || !this._editForm.valid;
    this.btnNext.disabled = this.saving || this.checking;
    this.btnBack.disabled = this.saving || this.checking;

    const bar: ButtonItem[] = [];
    if (this.currentStep > 0) bar.push(this.btnBack);
    bar.push(ultimo ? this.btnConfirm : this.btnNext);
    bar.push(this.btnCancel);
    this.buttonbar = bar;
  }

  // ── Salvataggio ──────────────────────────────────────────────────────────

  private handleConfirm = async () => {
    for (let step = 0; step < this.titoliStep.length; step++) {
      if (!this.stepValido(step)) {
        this.currentStep = step;
        this.updateButtonbar();
        return;
      }
    }

    this.SaveData(Object.assign(new UserAttendanceWizardModel(), this._editForm.value)).subscribe(res => {
      // inserimento completato: la lista utenti deve solo rileggersi
      if (res?.idAspNetUsers)
        this.modalCtrl.dismiss({ idAspNetUsers: res.idAspNetUsers, inserimentoCompletato: true }, 'confirm');
    });
  };

  private async salva(editModel: UserAttendanceWizardModel): Promise<UserAttendanceWizardModel | null> {
    this.saving = true;
    this.erroreSalvataggio = null;
    this.updateButtonbar();
    try {
      const esito = await this.nuovoDipendenteService.Inserisci(this.formUtente.value, {
        rapporto: this.formRapporto.value,
        profilo: this.formProfilo.value,
        competenze: this.competenzeSelezionate,
        maturazioni: this.maturazioni,
      });
      if (!esito.idAspNetUsers) {
        // errore sui dati utente (es. username o email già usati): si torna al primo step
        this.currentStep = 0;
      }
      if (!esito.ok) {
        this.erroreSalvataggio = esito.errore;
        return null;
      }
      this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Dip_Anagrafica);
      editModel.idAspNetUsers = esito.idAspNetUsers;
      return editModel;
    } finally {
      this.saving = false;
      this.updateButtonbar();
    }
  }
}

function minArrayLength(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    control.value && control.value.length >= min ? null : { minArrayLength: true };
}
