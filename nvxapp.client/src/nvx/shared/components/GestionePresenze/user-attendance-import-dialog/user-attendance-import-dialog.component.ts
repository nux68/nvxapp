import { Component, ChangeDetectionStrategy, ElementRef, OnInit } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonHeader, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonProgressBar, IonRow, IonSelect, IonSelectOption, IonText, IonTextarea, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { filter, firstValueFrom, Observable, of, take } from 'rxjs';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { UserCheckInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-check-model';
import { DipendentiImportService } from '../../../../ClientServer-Service/GestionePresenze/DipendentiImportService/dipendenti-import.service';
import { DIPENDENTI_IMPORT_MAX_BYTE, DipendenteImportDaInserireModel, DipendenteImportModel, DipendentiImport_EseguiInModel, DipendentiImport_EstraiInModel } from '../../../../ClientServer-Service/GestionePresenze/DipendentiImportService/Models/dipendenti-import-model';
import { NuovoDipendenteService } from '../../../../Utility/GestionePresenze/nuovo-dipendente.service';
import { codiceFiscaleValidator, normalizzaCodiceFiscale } from '../../../../Utility/GestionePresenze/codice-fiscale';
import { DipAnagraficaService } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { Dip_Anagrafica_Check_InModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { LongJobNotifierService } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { RefresherService } from '../../../../Utility/GestionePresenze/refresher.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Par_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';

type Fase = 'file' | 'analisi' | 'revisione' | 'salvataggio';

/** caratteri ammessi nello username (quelli predefiniti di ASP.NET Identity) */
const USERNAME_PATTERN = /^[A-Za-z0-9\-._@+]+$/;
/** lato maggiore delle foto inviate al LLM: bastano per leggere un documento e restano sotto i limiti dei provider */
const IMMAGINE_MAX_LATO = 2000;

/**
 * Importazione dei dipendenti da un file qualsiasi (lista utenti, FAB "+" > Importa da file):
 *  1 l'utente sceglie il file (anche una foto, es. di una carta d'identita') e puo' scrivere delle note
 *    per il LLM (es. "assunti tutti dal 1/11")
 *  2 il server ne legge il testo e il LLM estrae le persone con i dati del primo step del wizard
 *    (cognome, nome, username, email), la data di assunzione, la matricola e il codice fiscale
 *  3 l'elenco si corregge: con dati mancanti, non validi o gia' usati non si puo' importare;
 *    la password iniziale e' unica per tutti, ruoli, data di assunzione e profilo orario sono per
 *    dipendente (di default User, oggi e il profilo di default del wizard)
 *  4 l'elenco passa al server, che inserisce i dipendenti in un job (DipendentiImportEsegui) come dal
 *    wizard, con i valori di default per gli altri step: la data di assunzione e' anche l'inizio del
 *    profilo orario. Avanzamento e riepilogo (inseriti e motivi degli scarti) nella barra della pagina;
 *    il dialog si chiude con il JobId e la lista utenti si rilegge a fine job.
 * Standalone, come il wizard: la pagina di Infrastructure lo apre tramite DialogName.ImportUserCompany.
 */
@Component({
  selector: 'app-user-attendance-import-dialog',
  templateUrl: './user-attendance-import-dialog.component.html',
  styleUrls: ['./user-attendance-import-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonHeader, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonProgressBar, IonRow, IonSelect, IonSelectOption, IonText, IonTextarea, IonTitle, IonToolbar, SharedComponentInfrastructureModule]
})
export class UserAttendanceImportDialogComponent extends BaseDialogConfirmCancelComponent<any> implements OnInit {

  public fase: Fase = 'file';
  public nomeFile: string = '';
  /** indicazioni per il LLM scritte con la scelta del file */
  public note: string = '';
  public testoTroncato: boolean = false;
  /** errore della lettura del file o dell'avvio dell'inserimento */
  public errore: string | null = null;

  private btnAltroFile: ButtonItem;

  /** profili orari tra cui scegliere (come nello step 3 del wizard) */
  public par_ProfiloOrario: Par_ProfiloOrarioModel[] = [];

  public get dipendenti(): FormArray { return this._editForm?.get('dipendenti') as FormArray; }
  public get righe(): FormGroup[] { return (this.dipendenti?.controls ?? []) as FormGroup[]; }

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private parameterService: ParameterService,
              private accountService: AccountService,
              private dipendentiImportService: DipendentiImportService,
              private nuovoDipendenteService: NuovoDipendenteService,
              private dipAnagraficaService: DipAnagraficaService,
              private refresherService: RefresherService,
              private longJobNotifierService: LongJobNotifierService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private elementRef: ElementRef<HTMLElement>,
              private transloco: TranslocoService) {

    super(userInterfaceService, fb, modalCtrl);

    this.par_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario ?? [];

    this.btnAltroFile = new ButtonItem(this.t('altroFile'), 'document-outline', 'medium', false, () => this.ricomincia());
    this.btnConfirm.text = this.t('importa');
    this.btnConfirm.image = 'cloud-upload-outline';
    this.btnConfirm.event = this.handleConfirm;
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      pw: [null, [Validators.required]],
      dipendenti: this.fb.array([]),
    });
  }

  override ngOnInit() {
    super.ngOnInit();
    this.ingrandisciDialog();
    this._editForm.statusChanges.subscribe(() => this.updateButtonbar());
    this.updateButtonbar();
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.ingrandisciDialog();
  }

  get Title(): string { return this.t('title'); }

  LoadData = (): Observable<any> => of(this._editForm.value);

  SaveData = (editModel: any): Observable<any> => of(editModel);

  private t(key: string, params?: any): string {
    return this.transloco.translate('attendance.components.userAttendanceImportDialog.' + key, params);
  }

  /** Dialog piu' grande del normale da tablet in su (su mobile resta a tutto schermo). */
  private ingrandisciDialog(): void {
    const modal = this.elementRef.nativeElement.closest('ion-modal');
    if (modal && window.matchMedia('(min-width: 576px)').matches) {
      modal.style.setProperty('--width', 'min(1100px, 95vw)');
      modal.style.setProperty('--height', 'min(820px, 92vh)');
    }
  }

  // ── Ruoli: come nel wizard, User e CompanyAdmin ──────────────────────────

  getRoles(): string[] {
    return this.parameterService.Roles.filter(role => role.code == RoleCode.User || role.code == RoleCode.CompanyAdmin)
      .map(role => role.name);
  }

  // ── Step 1-2: scelta del file e lettura con il LLM ───────────────────────

  async fileScelto(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    this.errore = null;
    // le foto si riducono prima dell'invio: il limite vale per gli altri file (e lo ricontrolla il server)
    if (!file.type.startsWith('image/') && file.size > DIPENDENTI_IMPORT_MAX_BYTE) {
      this.errore = this.t('fileTroppoGrande', { mb: DIPENDENTI_IMPORT_MAX_BYTE / (1024 * 1024) });
      return;
    }

    this.nomeFile = file.name;
    this.fase = 'analisi';
    this.updateButtonbar();
    try {
      const request = new GenericRequest<DipendentiImport_EstraiInModel>(DipendentiImport_EstraiInModel);
      request.data.nomeFile = file.name;
      request.data.contenuto = file.type.startsWith('image/') ? await this.immagineBase64(file) : await this.base64(file);
      request.data.note = this.note.trim() || null;

      const res = await firstValueFrom(this.dipendentiImportService.DipendentiImportEstrai(request));
      const dipendenti = res?.success ? res.data?.dipendenti ?? [] : [];
      if (dipendenti.length === 0) {
        this.errore = res?.messages?.map(m => m.text).join(' ') || this.t('nessunDipendente');
        this.fase = 'file';
        return;
      }

      this.testoTroncato = !!res.data?.testoTroncato;
      this.caricaElenco(dipendenti);
      this.fase = 'revisione';
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      this.errore = this.t('erroreLettura');
      this.fase = 'file';
    } finally {
      this.updateButtonbar();
    }
  }

  private base64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '');
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }

  /**
   * Foto in JPEG ridotta (lato maggiore IMMAGINE_MAX_LATO): pesa meno ed e' un formato letto da tutti i
   * modelli con visione. Se il browser non sa disegnarla (es. HEIC fuori da Safari) parte cosi' com'e'.
   */
  private immagineBase64(file: File): Promise<string> {
    return new Promise(resolve => {
      const url = URL.createObjectURL(file);
      const img = new Image();
      img.onload = () => {
        URL.revokeObjectURL(url);
        const scala = Math.min(1, IMMAGINE_MAX_LATO / Math.max(img.naturalWidth, img.naturalHeight));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(img.naturalWidth * scala);
        canvas.height = Math.round(img.naturalHeight * scala);
        canvas.getContext('2d')?.drawImage(img, 0, 0, canvas.width, canvas.height);
        resolve(canvas.toDataURL('image/jpeg', 0.85).split(',')[1] ?? '');
      };
      img.onerror = () => {
        URL.revokeObjectURL(url);
        this.base64(file).then(resolve, () => resolve(''));
      };
      img.src = url;
    });
  }

  private caricaElenco(dipendenti: DipendenteImportModel[]): void {
    this.dipendenti.clear();
    for (const d of dipendenti) {
      const riga = this.nuovaRiga(d);
      this.dipendenti.push(riga);
      // gia' usati in archivio (controllo del server): l'errore sparisce modificando il campo
      if (d.userNameExists) this.segnaGiaUsato(riga, 'descrizione', true);
      if (d.mailExists) this.segnaGiaUsato(riga, 'mail', true);
      if (d.matricolaExists) this.segnaGiaUsato(riga, 'matricola', true);
      if (d.codiceFiscaleExists) this.segnaGiaUsato(riga, 'codiceFiscale', true);
    }
    this.dipendenti.markAllAsTouched();
  }

  private nuovaRiga(d?: Partial<DipendenteImportModel>): FormGroup {
    const userRole = this.parameterService.Roles.find(role => role.code === RoleCode.User);
    return this.fb.group({
      cognome: [d?.cognome || null, [Validators.required, Validators.maxLength(50)]],
      nome: [d?.nome || null, [Validators.required, Validators.maxLength(50)]],
      descrizione: [d?.userName || null, [Validators.required, Validators.maxLength(50), Validators.pattern(USERNAME_PATTERN)]],
      mail: [d?.mail || null, [Validators.required, Validators.email]],
      roles: [userRole ? [userRole.name] : [], [minArrayLength(1)]],
      // "YYYY-MM-DD" (campo data); vuota = oggi
      dataAss: [d?.dataAss ? d.dataAss.substring(0, 10) : null],
      // facoltativi
      matricola: [d?.matricola || null, [Validators.maxLength(20)]],
      codiceFiscale: [d?.codiceFiscale || null, [codiceFiscaleValidator()]],
      idPar_ProfiloOrario: [this.nuovoDipendenteService.Default().profilo.idPar_ProfiloOrario, [Validators.required]],
    });
  }

  private ricomincia(): void {
    if (this.fase === 'salvataggio') return;
    this.dipendenti.clear();
    this.errore = null;
    this.testoTroncato = false;
    this.fase = 'file';
    this.updateButtonbar();
  }

  // ── Step 3: correzione dell'elenco ───────────────────────────────────────

  aggiungiRiga(): void {
    this.dipendenti.push(this.nuovaRiga());
  }

  togliRiga(index: number): void {
    this.dipendenti.removeAt(index);
  }

  /** stesso username, email, matricola o codice fiscale in un'altra riga dell'elenco */
  duplicato(index: number, campo: CampoUnico): boolean {
    const valore = normalizza(this.righe[index]?.get(campo)?.value);
    return !!valore && this.righe.some((r, i) => i !== index && normalizza(r.get(campo)?.value) === valore);
  }

  /** riga con un campo ripetuto in un'altra riga */
  rigaDuplicata(index: number): boolean {
    return CAMPI_UNICI.some(campo => this.duplicato(index, campo));
  }

  private get senzaDuplicati(): boolean {
    return this.righe.every((_, i) => !this.rigaDuplicata(i));
  }

  public get importabile(): boolean {
    return this.righe.length > 0 && this._editForm.valid && this.senzaDuplicati;
  }

  private updateButtonbar(): void {
    const occupato = this.fase === 'analisi' || this.fase === 'salvataggio';
    this.btnConfirm.disabled = occupato || !this.importabile;
    this.btnAltroFile.disabled = occupato;
    this.btnCancel.disabled = this.fase === 'salvataggio';

    const bar: ButtonItem[] = [];
    if (this.fase === 'revisione' || this.fase === 'salvataggio') bar.push(this.btnAltroFile, this.btnConfirm);
    bar.push(this.btnCancel);
    this.buttonbar = bar;
  }

  // ── Step 4: inserimento in un job del server ─────────────────────────────

  /**
   * Controlla i dati e passa l'elenco al server, che inserisce i dipendenti in un job: l'avanzamento e il
   * riepilogo finale si vedono nella barra della pagina; il dialog si chiude e restituisce il JobId
   * (la lista utenti si rilegge a fine job).
   */
  private handleConfirm = async () => {
    this._editForm.markAllAsTouched();
    if (this.fase !== 'revisione' || !this.importabile) return;

    this.fase = 'salvataggio';
    this.errore = null;
    this.updateButtonbar();
    try {
      if (!(await this.verificaUsernameMail()) || !(await this.verificaMatricolaCodiceFiscale())) {
        this.errore = this.t('giaUsati');
        return;
      }

      const request = new GenericRequest<DipendentiImport_EseguiInModel>(DipendentiImport_EseguiInModel);
      request.data.password = this._editForm.value.pw;
      request.data.dipendenti = this.righe.map(r => {
        const v = r.value;
        const d = new DipendenteImportDaInserireModel();
        d.cognome = v.cognome.trim();
        d.nome = v.nome.trim();
        d.userName = v.descrizione.trim();
        d.mail = v.mail.trim();
        d.roles = v.roles;
        // data di assunzione (anche inizio del profilo orario); vuota = oggi
        d.dataAss = v.dataAss ? v.dataAss.substring(0, 10) + 'T00:00:00' : null;
        d.idPar_ProfiloOrario = v.idPar_ProfiloOrario;
        d.matricola = v.matricola?.trim() || null;
        d.codiceFiscale = normalizzaCodiceFiscale(v.codiceFiscale) || null;
        return d;
      });

      const res = await firstValueFrom(this.dipendentiImportService.DipendentiImportEsegui(request));
      const jobId = res?.success ? res.data?.jobId : null;
      if (!jobId) {
        this.errore = res?.messages?.map(m => m.text).join(' ') || this.t('erroreAvvio');
        return;
      }

      // a fine job i dipendenti nuovi devono comparire anche nelle tabelle delle presenze
      this.longJobNotifierService.jobFinished$
        .pipe(filter(job => job.jobId === jobId), take(1))
        .subscribe(() => this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Dip_Anagrafica));

      this.modalCtrl.dismiss({ jobId }, 'confirm');
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      this.errore = this.t('erroreAvvio');
    } finally {
      if (this.fase === 'salvataggio') this.fase = 'revisione';
      this.updateButtonbar();
    }
  };

  /**
   * Controlla in archivio username ed email di tutte le righe (possono essere stati corretti):
   * quelli gia' usati si segnano sul campo. In caso di errore della chiamata non blocca:
   * il controllo definitivo resta alla creazione dell'utente.
   */
  private async verificaUsernameMail(): Promise<boolean> {
    let liberi = true;
    for (const riga of this.righe) {
      const request = new GenericRequest<UserCheckInModel>(UserCheckInModel);
      request.data.userName = riga.value.descrizione;
      request.data.mail = riga.value.mail;
      try {
        const res = await firstValueFrom(this.accountService.UserCheck(request));
        if (!res?.success || !res.data) continue;
        this.segnaGiaUsato(riga, 'descrizione', res.data.userNameExists);
        this.segnaGiaUsato(riga, 'mail', res.data.mailExists);
        liberi = liberi && !res.data.userNameExists && !res.data.mailExists;
      } catch (error) {
        console.error('Errore durante la chiamata API:', error);
      }
    }
    return liberi;
  }

  /**
   * Controlla matricola e codice fiscale delle righe che li hanno: quelli gia' di un altro dipendente
   * dell'azienda si segnano sul campo. In caso di errore della chiamata non blocca: il server li ricontrolla.
   */
  private async verificaMatricolaCodiceFiscale(): Promise<boolean> {
    let liberi = true;
    for (const riga of this.righe) {
      const matricola = (riga.value.matricola ?? '').trim();
      const codiceFiscale = normalizzaCodiceFiscale(riga.value.codiceFiscale);
      if (!matricola && !codiceFiscale) continue;
      const request = new GenericRequest<Dip_Anagrafica_Check_InModel>(Dip_Anagrafica_Check_InModel);
      request.data.matricola = matricola || null;
      request.data.codiceFiscale = codiceFiscale || null;
      try {
        const res = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaCheck(request));
        if (!res?.success || !res.data) continue;
        this.segnaGiaUsato(riga, 'matricola', res.data.matricolaExists);
        this.segnaGiaUsato(riga, 'codiceFiscale', res.data.codiceFiscaleExists);
        liberi = liberi && !res.data.matricolaExists && !res.data.codiceFiscaleExists;
      } catch (error) {
        console.error('Errore durante la chiamata API:', error);
      }
    }
    return liberi;
  }

  /** Aggiunge o toglie l'errore "giaUsato" lasciando gli altri errori del campo. */
  private segnaGiaUsato(riga: FormGroup, campo: string, giaUsato: boolean): void {
    const control = riga.get(campo);
    if (!control) return;
    const { giaUsato: _, ...altri } = control.errors ?? {};
    const errori = giaUsato ? { ...altri, giaUsato: true } : altri;
    control.setErrors(Object.keys(errori).length ? errori : null);
    control.markAsTouched();
  }
}

/** campi che non si possono ripetere tra le righe (vuoti esclusi) */
type CampoUnico = 'descrizione' | 'mail' | 'matricola' | 'codiceFiscale';
const CAMPI_UNICI: CampoUnico[] = ['descrizione', 'mail', 'matricola', 'codiceFiscale'];

function normalizza(valore: string | null | undefined): string {
  return (valore ?? '').replace(/\s/g, '').toLowerCase();
}

function minArrayLength(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    control.value && control.value.length >= min ? null : { minArrayLength: true };
}
