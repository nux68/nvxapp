import { Component, ChangeDetectionStrategy, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { IonicModule, ModalController } from '@ionic/angular/lazy';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { RefresherService } from '../../../../Utility/GestionePresenze/refresher.service';
import { firstValueFrom } from 'rxjs';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { UserCompanyEditModel, UserCompanyPutInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { UserCheckInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-check-model';
import { DipAnagraficaService } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Put_InModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DestinazioneEccedenze, TipoContratto } from '../../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { Dip_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { Dip_CompetenzaModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Competenza/Models/dip-competenza-model';
import { Dip_Rapporto_Giustificativi_MaturazioneModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Rapporto_Giustificativi_Maturazione/Models/dip-rapporto-giustificativi-maturazione-model';
import { Par_GiustificativiModel, TipoContatore } from '../../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { Par_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { Par_CompetenzaModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';

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
 */
@Component({
  selector: 'app-user-attendance-wizard-dialog',
  templateUrl: './user-attendance-wizard-dialog.component.html',
  styleUrls: ['./user-attendance-wizard-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule, SharedComponentInfrastructureModule]
})
export class UserAttendanceWizardDialogComponent implements OnInit {

  public readonly titoliStep = ['Utente', 'Rapporto di lavoro', 'Profilo orario', 'Competenze', 'Maturazione contatori'];
  public readonly tipoContrattoEnum = TipoContratto;
  public readonly destinazioneEccedenzeEnum = DestinazioneEccedenze;

  public currentStep: number = 0;
  public saving: boolean = false;
  /** verifica in corso di username ed email (passaggio dallo step 1 allo step 2) */
  public checking: boolean = false;
  public erroreSalvataggio: string | null = null;

  public buttonbar: ButtonItem[] = [];
  private btnNext: ButtonItem;
  private btnBack: ButtonItem;
  private btnConfirm: ButtonItem;
  private btnCancel: ButtonItem;

  // un form per step: si valida solo quello corrente
  public formUtente: FormGroup;
  public formRapporto: FormGroup;
  public formProfilo: FormGroup;

  /** Competenze selezionate (step 4). */
  public competenzeSelezionate: number[] = [];
  /** Ore di maturazione mensile per giustificativo-contatore (step 5), formato "HHH:MM". */
  public maturazioni: { [idPar_Giustificativi: number]: string } = {};

  public par_ProfiloOrario: Par_ProfiloOrarioModel[] = [];
  public par_Competenza: Par_CompetenzaModel[] = [];
  public giustificativiContatore: Par_GiustificativiModel[] = [];

  constructor(private userInterfaceService: UserInterfaceService,
              private fb: FormBuilder,
              private modalCtrl: ModalController,
              private parameterService: ParameterService,
              private accountService: AccountService,
              private dipAnagraficaService: DipAnagraficaService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private refresherService: RefresherService,
              private elementRef: ElementRef<HTMLElement>) {

    this.btnNext = new ButtonItem('Avanti', 'arrow-forward-outline', 'primary', false, this.handleNext);
    this.btnBack = new ButtonItem('Indietro', 'arrow-back-outline', 'medium', false, this.handleBack);
    this.btnConfirm = this.userInterfaceService.Btn_Conferma;
    this.btnConfirm.event = this.handleConfirm;
    this.btnCancel = this.userInterfaceService.Btn_Annulla;
    this.btnCancel.event = this.handleCancel;
  }

  ngOnInit() {
    this.ingrandisciDialog();

    const userRole = this.parameterService.Roles.find(role => role.code === RoleCode.User);

    this.formUtente = this.fb.group({
      cognome: [null, [Validators.required, Validators.maxLength(50)]],
      nome: [null, [Validators.required, Validators.maxLength(50)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      pw: [null, [Validators.required]],
      mail: [null, [Validators.required, Validators.email]],
      roles: [userRole ? [userRole.name] : [], [minArrayLength(1)]],
    });

    this.formRapporto = this.fb.group({
      dataAss: [oggiIso(), [Validators.required]],
      tipoContratto: [TipoContratto.FullTime],
      oreSettimanaliContratto: [null, [Validators.min(0), Validators.max(168)]],
      ccnl: [null, [Validators.maxLength(50)]],
      destinazioneEccedenze: [DestinazioneEccedenze.Pagamento],
    });

    this.par_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario ?? [];
    const profiloDefault = this.par_ProfiloOrario.find(p => p.codice === 'DAY7H8') ?? this.par_ProfiloOrario[0];
    this.formProfilo = this.fb.group({
      idPar_ProfiloOrario: [profiloDefault?.id ?? null, [Validators.required]],
      numGiornoPartenzaCiclo: [1, [Validators.required, Validators.min(1)]],
    });

    this.par_Competenza = this.sharedParameterGestionePresenzeService.Par_Competenza ?? [];
    this.competenzeSelezionate = this.par_Competenza.filter(c => c.default).map(c => c.id);

    this.giustificativiContatore = (this.sharedParameterGestionePresenzeService.Par_Giustificativi ?? [])
      .filter(j => j.tipoContatore !== TipoContatore.NoContatore);
    this.giustificativiContatore.forEach(j => this.maturazioni[j.id] = '00:00');

    this.updateButtonbar();
  }

  get Title(): string { return 'Nuovo dipendente'; }

  // all'apertura il componente e' gia' dentro ion-modal (in ngOnInit puo' non esserlo ancora)
  ionViewWillEnter() {
    this.ingrandisciDialog();
  }

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
    // dallo step 1: username ed email non devono essere già usati da un altro utente
    if (this.currentStep === 0 && !(await this.verificaUsernameMail())) return;
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

  private handleCancel = () => {
    this.modalCtrl.dismiss(null, 'cancel');
  };

  private updateButtonbar(): void {
    const ultimo = this.currentStep === this.titoliStep.length - 1;
    this.btnConfirm.disabled = this.saving;
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

    this.saving = true;
    this.erroreSalvataggio = null;
    this.updateButtonbar();
    try {
      const idAspNetUsers = await this.CreaUtente();
      if (!idAspNetUsers) {
        // errore sui dati utente (es. username o email già usati): si torna al primo step
        this.currentStep = 0;
        return;
      }
      if (await this.SalvaDatiPresenze(idAspNetUsers)) {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        // inserimento completato: la lista utenti deve solo rileggersi
        this.modalCtrl.dismiss({ idAspNetUsers, inserimentoCompletato: true }, 'confirm');
      }
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      this.erroreSalvataggio = 'Salvataggio non riuscito.';
    } finally {
      this.saving = false;
      this.updateButtonbar();
    }
  };

  /** Crea l'utente nell'azienda: il server prepara anagrafica, rapporto e profilo di default. */
  private async CreaUtente(): Promise<string | null> {
    const v = this.formUtente.value;
    const request = new GenericRequest<UserCompanyPutInModel>(UserCompanyPutInModel);
    request.data.userCompanyEdit = new UserCompanyEditModel(v.descrizione, 0, '', false, v.mail, v.pw, v.roles);

    const res = await firstValueFrom(this.accountService.UserCompanyPut(request));
    const idAspNetUsers = res?.success ? res.data?.userCompanyEdit?.idAspNetUsers : null;
    if (!idAspNetUsers) {
      this.erroreSalvataggio = res?.messages?.map(m => m.text).join(' ') || 'Creazione utente non riuscita.';
      return null;
    }
    return idAspNetUsers;
  }

  /** Applica ai dati creati dal server quelli inseriti negli step 2-5 e li salva. */
  private async SalvaDatiPresenze(idAspNetUsers: string): Promise<boolean> {
    const reqGet = new GenericRequest<Dip_Anagrafica_Get_InModel>(Dip_Anagrafica_Get_InModel);
    reqGet.data.id = idAspNetUsers;
    const resGet = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaGet(reqGet));
    const dip = resGet?.data?.dip_Anagrafica;
    const rapporto = dip?.dip_RapportoLavoro?.[0];
    if (!dip || !rapporto) {
      this.erroreSalvataggio = 'Utente creato, ma i dati del dipendente non sono disponibili: completarli dalla sua scheda.';
      return false;
    }

    // step 1
    dip.cognome = this.formUtente.value.cognome;
    dip.nome = this.formUtente.value.nome;
    dip.pw = '';

    // step 2: rapporto di lavoro (la data di licenziamento resta vuota)
    const r = this.formRapporto.value;
    rapporto.dataAss = r.dataAss;
    rapporto.tipoContratto = r.tipoContratto;
    rapporto.oreSettimanaliContratto = r.tipoContratto === TipoContratto.PartTime && r.oreSettimanaliContratto !== null && r.oreSettimanaliContratto !== ''
      ? +r.oreSettimanaliContratto : null;
    rapporto.ccnl = r.ccnl || null;
    rapporto.destinazioneEccedenze = r.destinazioneEccedenze;

    // step 3: profilo orario dalla data di assunzione (si aggiorna quello di default creato dal server)
    const p = this.formProfilo.value;
    let profilo = dip.dip_ProfiloOrario.find(x => x.idDip_RapportoLavoro === rapporto.id);
    if (!profilo) {
      profilo = new Dip_ProfiloOrarioModel();
      profilo.id = -1;
      profilo.idDip_RapportoLavoro = rapporto.id;
      profilo.al = '2099-12-31T00:00:00';
      dip.dip_ProfiloOrario.push(profilo);
    }
    profilo.idPar_ProfiloOrario = p.idPar_ProfiloOrario;
    profilo.numGiornoPartenzaCiclo = +p.numGiornoPartenzaCiclo;
    profilo.dal = r.dataAss;
    dip.dip_ProfiloOrario = dip.dip_ProfiloOrario.filter(x => x.idDip_RapportoLavoro !== rapporto.id || x === profilo);

    // step 4: competenze (senza competenze il server assegna quella di default)
    dip.dip_Competenza = this.competenzeSelezionate.map(idPar_Competenza => {
      const c = new Dip_CompetenzaModel();
      c.id = 0;
      c.idDip_Anagrafica = dip.id;
      c.idPar_Competenza = idPar_Competenza;
      return c;
    });

    // step 5: maturazione mensile, solo i giustificativi valorizzati
    dip.dip_Maturazione = this.giustificativiContatore
      .filter(j => this.maturazioni[j.id] && this.maturazioni[j.id] !== '00:00' && this.maturazioni[j.id] !== '0:00')
      .map(j => {
        const m = new Dip_Rapporto_Giustificativi_MaturazioneModel();
        m.idDip_RapportoLavoro = rapporto.id;
        m.idPar_Giustificativi = j.id;
        m.oreMaturazione = this.maturazioni[j.id];
        return m;
      });

    const reqPut = new GenericRequest<Dip_Anagrafica_Put_InModel>(Dip_Anagrafica_Put_InModel);
    reqPut.data.dip_Anagrafica = dip;
    reqPut.data.id = dip.idAspNetUsers;
    const resPut = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaPut(reqPut));
    if (!resPut?.success) {
      this.erroreSalvataggio = resPut?.messages?.map(m => m.text).join(' ') ||
        'Utente creato, ma il salvataggio dei dati del dipendente non è riuscito: completarli dalla sua scheda.';
      return false;
    }
    return true;
  }
}

/** Oggi nel formato delle date del rapporto ("YYYY-MM-DDT00:00:00", senza fuso orario). */
function oggiIso(): string {
  const d = new Date();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${mm}-${dd}T00:00:00`;
}

function minArrayLength(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    control.value && control.value.length >= min ? null : { minArrayLength: true };
}
