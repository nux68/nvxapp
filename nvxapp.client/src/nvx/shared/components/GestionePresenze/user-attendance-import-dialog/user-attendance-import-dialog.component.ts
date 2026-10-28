import { Component, ChangeDetectionStrategy, ElementRef, OnInit } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonHeader, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonProgressBar, IonRow, IonSelect, IonSelectOption, IonText, IonTextarea, IonTitle, IonToolbar, ModalController } from '@ionic/angular';
import { firstValueFrom, Observable, of } from 'rxjs';
import { SharedComponentInfrastructureModule } from '../../../shared-component-infrastructure.module';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { AccountService } from '../../../../ClientServer-Service/Infrastructure/Account/account.service';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { UserCheckInModel } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-check-model';
import { DipendentiImportService } from '../../../../ClientServer-Service/GestionePresenze/DipendentiImportService/dipendenti-import.service';
import { DIPENDENTI_IMPORT_MAX_BYTE, DipendenteImportModel, DipendentiImport_EstraiInModel } from '../../../../ClientServer-Service/GestionePresenze/DipendentiImportService/Models/dipendenti-import-model';
import { NuovoDipendenteService, NuovoDipendenteUtente } from '../../../../Utility/GestionePresenze/nuovo-dipendente.service';
import { RefresherService } from '../../../../Utility/GestionePresenze/refresher.service';
import { ParTable } from '../../../shared-parameter-gestione-presenze.service';
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
 *    (cognome, nome, username, email) e la data di assunzione
 *  3 l'elenco si corregge: con dati mancanti, non validi o gia' usati non si puo' importare;
 *    la password iniziale e' unica per tutti, ruoli e data di assunzione sono per dipendente
 *    (di default User e oggi)
 *  4 ogni dipendente si inserisce come dal wizard, con i valori di default per gli altri step
 *    (NuovoDipendenteService): la data di assunzione e' anche l'inizio del profilo orario.
 *    Quelli non inseriti restano in elenco con l'errore.
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
  /** errore della lettura del file o dell'ultimo inserimento */
  public errore: string | null = null;
  /** errore di inserimento per dipendente (stessa posizione dell'elenco) */
  public erroriRiga: (string | null)[] = [];
  /** dipendenti gia' inseriti in questa importazione */
  public inseriti: number = 0;
  /** avanzamento dell'inserimento */
  public progresso: number = 0;

  private btnAltroFile: ButtonItem;

  public get dipendenti(): FormArray { return this._editForm?.get('dipendenti') as FormArray; }
  public get righe(): FormGroup[] { return (this.dipendenti?.controls ?? []) as FormGroup[]; }

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private parameterService: ParameterService,
              private accountService: AccountService,
              private dipendentiImportService: DipendentiImportService,
              private nuovoDipendenteService: NuovoDipendenteService,
              private refresherService: RefresherService,
              private elementRef: ElementRef<HTMLElement>,
              private transloco: TranslocoService) {

    super(userInterfaceService, fb, modalCtrl);

    this.btnAltroFile = new ButtonItem(this.t('altroFile'), 'document-outline', 'medium', false, () => this.ricomincia());
    this.btnConfirm.text = this.t('importa');
    this.btnConfirm.image = 'cloud-upload-outline';
    this.btnConfirm.event = this.handleConfirm;
    // dopo inserimenti parziali la lista utenti deve rileggersi anche chiudendo
    this.btnCancel.event = () => this.chiudi();
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
    this.erroriRiga = [];
    for (const d of dipendenti) {
      const riga = this.nuovaRiga(d);
      this.dipendenti.push(riga);
      this.erroriRiga.push(null);
      // gia' usati in archivio (controllo del server): l'errore sparisce modificando il campo
      if (d.userNameExists) this.segnaGiaUsato(riga, 'descrizione', true);
      if (d.mailExists) this.segnaGiaUsato(riga, 'mail', true);
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
    });
  }

  private ricomincia(): void {
    if (this.fase === 'salvataggio') return;
    this.dipendenti.clear();
    this.erroriRiga = [];
    this.errore = null;
    this.testoTroncato = false;
    this.fase = 'file';
    this.updateButtonbar();
  }

  // ── Step 3: correzione dell'elenco ───────────────────────────────────────

  aggiungiRiga(): void {
    const riga = this.nuovaRiga();
    this.dipendenti.push(riga);
    this.erroriRiga.push(null);
  }

  togliRiga(index: number): void {
    this.dipendenti.removeAt(index);
    this.erroriRiga.splice(index, 1);
  }

  /** stesso username o email in un'altra riga dell'elenco */
  duplicato(index: number, campo: 'descrizione' | 'mail'): boolean {
    const valore = normalizza(this.righe[index]?.get(campo)?.value);
    return !!valore && this.righe.some((r, i) => i !== index && normalizza(r.get(campo)?.value) === valore);
  }

  private get senzaDuplicati(): boolean {
    return this.righe.every((_, i) => !this.duplicato(i, 'descrizione') && !this.duplicato(i, 'mail'));
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

  // ── Step 4: inserimento ──────────────────────────────────────────────────

  private handleConfirm = async () => {
    this._editForm.markAllAsTouched();
    if (this.fase !== 'revisione' || !this.importabile) return;

    this.fase = 'salvataggio';
    this.errore = null;
    this.progresso = 0;
    this.updateButtonbar();
    try {
      if (!(await this.verificaUsernameMail())) {
        this.errore = this.t('giaUsati');
        return;
      }

      const pw: string = this._editForm.value.pw;
      const altriDati = this.nuovoDipendenteService.Default();
      const righe = [...this.righe];
      const daTogliere: FormGroup[] = [];
      const avvisi: string[] = [];

      for (let i = 0; i < righe.length; i++) {
        const v = righe[i].value;
        const utente: NuovoDipendenteUtente = {
          cognome: v.cognome.trim(), nome: v.nome.trim(), descrizione: v.descrizione.trim(), mail: v.mail.trim(), pw, roles: v.roles
        };
        // data di assunzione della riga (anche inizio del profilo orario), altrimenti oggi
        const dati = { ...altriDati, rapporto: { ...altriDati.rapporto, dataAss: v.dataAss ? v.dataAss.substring(0, 10) + 'T00:00:00' : altriDati.rapporto.dataAss } };
        const esito = await this.nuovoDipendenteService.Inserisci(utente, dati);

        if (esito.idAspNetUsers) {
          // utente creato: esce dall'elenco anche se i dati delle presenze non si sono salvati (si completano dalla scheda)
          this.inseriti++;
          daTogliere.push(righe[i]);
          if (!esito.ok)
            avvisi.push(this.t('datiPresenzeNonSalvati', { nome: `${utente.cognome} ${utente.nome}`, errore: esito.errore }));
        } else {
          this.erroriRiga[this.righe.indexOf(righe[i])] = esito.errore;
        }
        this.progresso = (i + 1) / righe.length;
      }

      for (const riga of daTogliere) {
        const index = this.righe.indexOf(riga);
        this.dipendenti.removeAt(index);
        this.erroriRiga.splice(index, 1);
      }
      if (this.inseriti > 0)
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Dip_Anagrafica);

      if (this.righe.length === 0 && avvisi.length === 0) {
        this.modalCtrl.dismiss({ inserimentoCompletato: true, inseriti: this.inseriti }, 'confirm');
        return;
      }
      this.errore = [this.t('esito', { inseriti: this.inseriti, rimasti: this.righe.length }), ...avvisi].join(' ');
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

  /** Aggiunge o toglie l'errore "giaUsato" lasciando gli altri errori del campo. */
  private segnaGiaUsato(riga: FormGroup, campo: string, giaUsato: boolean): void {
    const control = riga.get(campo);
    if (!control) return;
    const { giaUsato: _, ...altri } = control.errors ?? {};
    const errori = giaUsato ? { ...altri, giaUsato: true } : altri;
    control.setErrors(Object.keys(errori).length ? errori : null);
    control.markAsTouched();
  }

  private chiudi(): void {
    if (this.inseriti > 0)
      this.modalCtrl.dismiss({ inserimentoCompletato: true, inseriti: this.inseriti }, 'confirm');
    else
      this.modalCtrl.dismiss(null, 'cancel');
  }
}

function normalizza(valore: string | null | undefined): string {
  return (valore ?? '').trim().toLowerCase();
}

function minArrayLength(min: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    control.value && control.value.length >= min ? null : { minArrayLength: true };
}
