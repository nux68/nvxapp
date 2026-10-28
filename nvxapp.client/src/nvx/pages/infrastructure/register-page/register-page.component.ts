import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, Subscription, catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RegistrationService } from '../../../ClientServer-Service/Infrastructure/Registration/registration.service';
import { CompanyRegisterInModel, CompanyRegisterUserNamesCheckInModel } from '../../../ClientServer-Service/Infrastructure/Registration/Models/company-register-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';

/*
 Showcase: registrazione di una nuova azienda (prima del login). Il server salva la richiesta e invia
 all'email indicata il link di conferma: l'azienda nasce (nel ramo DEMO, con i suoi utenti principali e,
 nei rami applicativi, gli applicativi previsti per le nuove aziende) solo aprendo il link, nella
 pagina /confirm-email (ConfirmEmailPageComponent).
 Digitata la ragione sociale si mostrano i nomi degli amministratori che verranno creati (power admin e admin,
 predefiniti <nome>_PowerAdmin e <nome>_Admin): si possono cambiare e vengono verificati sul server
 (validi e non gia' in uso). Gli altri utenti dell'azienda mantengono i nomi predefiniti.
*/
@Component({
  selector: 'app-register-page',
  templateUrl: './register-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class RegisterPageComponent extends BasePageConfirmCancelComponent<CompanyRegisterInModel> {

  // stessi limiti del server (RegistrationService)
  public readonly passwordMinLength = 8;
  public readonly ragioneSocialeMaxLength = 40;
  public readonly userNameMinLength = 3;
  public readonly userNameMaxLength = 50;
  private static readonly userNamePattern = /^[A-Za-z0-9_.-]+$/;

  public errori: string[] = [];
  // nomi degli amministratori in verifica sul server
  public verificaNomi = false;
  private subNomi: Subscription[] = [];
  // richiesta inviata: indirizzo a cui e' arrivato il link e sua validita'
  public mailInviata: string | null = null;
  public scadenzaOre = 0;

  private btnHome: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private registrationService: RegistrationService,
              public showcaseService: ShowcaseService,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnConfirm.text = this.transloco.translate('pages.register.signUp');
    this.btnConfirm.image = 'person-add-outline';
    this.btnConfirm.disabled = true;
    this.btnHome = new ButtonItem(this.transloco.translate('pages.register.home'), 'home-outline', 'primary', false, () => this.navCtrl.navigateRoot('/home'));
  }

  get Title(): string { return this.transloco.translate('pages.register.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      ragioneSociale: ['', [Validators.required, Validators.maxLength(this.ragioneSocialeMaxLength), RegisterPageComponent.lettereOCifre]],
      mail: ['', [Validators.required, Validators.email]],
      pw: ['', [Validators.required, Validators.minLength(this.passwordMinLength)]],
      conferma: ['', [Validators.required]],
      accettaCondizioni: [false, [Validators.requiredTrue]],
      powerAdminUserName: ['', this.userNameValidators],
      adminUserName: ['', this.userNameValidators]
    }, { validators: RegisterPageComponent.passwordUguali });
  }

  private get userNameValidators() {
    return [Validators.required, Validators.minLength(this.userNameMinLength), Validators.maxLength(this.userNameMaxLength), Validators.pattern(RegisterPageComponent.userNamePattern)];
  }

  override ngOnInit() {
    super.ngOnInit();
    const ctrl = this._editForm.controls;

    // nomi predefiniti degli amministratori finche' non li si cambia a mano
    this.subNomi.push(ctrl['ragioneSociale'].valueChanges.subscribe(() => {
      const nome = this.nomeAzienda;
      if (!ctrl['powerAdminUserName'].dirty)
        ctrl['powerAdminUserName'].setValue(nome.length >= 3 ? nome + '_PowerAdmin' : '');
      if (!ctrl['adminUserName'].dirty)
        ctrl['adminUserName'].setValue(nome.length >= 3 ? nome + '_Admin' : '');
    }));

    // verifica sul server (validi e non gia' in uso) quando i nomi sono formalmente corretti:
    // l'esito diventa l'errore 'nonDisponibile' del campo (il server ricontrolla comunque alla registrazione)
    this.subNomi.push(this._editForm.valueChanges.pipe(
      map(v => ({ ragioneSociale: v.ragioneSociale ?? '', powerAdminUserName: v.powerAdminUserName ?? '', adminUserName: v.adminUserName ?? '' })),
      distinctUntilChanged((a, b) => a.ragioneSociale === b.ragioneSociale && a.powerAdminUserName === b.powerAdminUserName && a.adminUserName === b.adminUserName),
      debounceTime(400),
      switchMap(v => {
        if (ctrl['ragioneSociale'].invalid || ctrl['powerAdminUserName'].hasError('pattern') || ctrl['adminUserName'].hasError('pattern')
            || !ctrl['powerAdminUserName'].value || !ctrl['adminUserName'].value)
          return of(null);
        const request = new GenericRequest<CompanyRegisterUserNamesCheckInModel>(CompanyRegisterUserNamesCheckInModel);
        request.data.ragioneSociale = v.ragioneSociale;
        request.data.powerAdminUserName = v.powerAdminUserName;
        request.data.adminUserName = v.adminUserName;
        this.verificaNomi = true;
        return this.registrationService.CompanyRegisterUserNamesCheck(request).pipe(catchError(() => of(null)));
      })
    ).subscribe(res => {
      this.verificaNomi = false;
      if (!res?.success || !res.data)
        return;
      this.impostaErroreNome('powerAdminUserName', res.data.powerAdminUserNameError);
      this.impostaErroreNome('adminUserName', res.data.adminUserNameError);
    }));
  }

  ngOnDestroy() {
    this.subNomi.forEach(s => s.unsubscribe());
  }

  // aggiunge/toglie l'errore del server senza toccare quelli dei validatori del campo
  private impostaErroreNome(campo: string, messaggio: string) {
    const control = this._editForm.controls[campo];
    const { nonDisponibile, ...altri } = control.errors ?? {};
    const errori = messaggio ? { ...altri, nonDisponibile: messaggio } : altri;
    control.setErrors(Object.keys(errori).length > 0 ? errori : null);
    if (messaggio)
      control.markAsTouched();
  }

  /** messaggio da mostrare sotto il campo del nome di un amministratore (null = nessun errore) */
  public erroreNome(campo: string): string | null {
    const control = this._editForm.controls[campo];
    if (control.hasError('nonDisponibile'))
      return control.getError('nonDisponibile');
    if (control.invalid && (control.dirty || control.touched))
      return this.transloco.translate('pages.register.userNameError', { min: this.userNameMinLength, max: this.userNameMaxLength });
    return null;
  }

  // pagina pubblica: annullando si torna alla home
  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateRoot('/home');
  }

  LoadData = (): Observable<CompanyRegisterInModel> => {
    return of(new CompanyRegisterInModel());
  };

  SaveData = (editModel: CompanyRegisterInModel): Observable<boolean> => {
    this.errori = [];
    const request = new GenericRequest<CompanyRegisterInModel>(CompanyRegisterInModel);
    request.data.ragioneSociale = editModel.ragioneSociale;
    request.data.mail = editModel.mail;
    request.data.pw = editModel.pw;
    request.data.accettaCondizioni = editModel.accettaCondizioni === true;
    request.data.powerAdminUserName = editModel.powerAdminUserName;
    request.data.adminUserName = editModel.adminUserName;

    this.btnConfirm.disabled = true;
    return this.registrationService.CompanyRegister(request).pipe(map(res => {
      this.btnConfirm.disabled = !this._editForm.valid;
      if (!res.success)
        return false; // errore inatteso: lo mostra l'interceptor
      // dati non validi o nome gia' usato: messaggi del server
      const messaggi = res.data?.messages ?? [];
      if (messaggi.length > 0) {
        this.errori = messaggi.map(m => m.text);
        return false;
      }
      this.mailInviata = res.data.mail;
      this.scadenzaOre = res.data.scadenzaOre;
      return true;
    }));
  };

  // richiesta inviata: si resta sulla pagina con l'esito (l'account si attiva dal link ricevuto per email)
  override AfterSave(saved: boolean): void {
    if (saved)
      this.buttonbar = [this.btnHome];
  }

  public get c(): ShowcaseContenuti['register'] {
    return this.showcaseService.Contenuti.register;
  }

  /** nome con cui nasce l'azienda: il server tiene solo lettere e cifre (anche per i nomi degli utenti) */
  public get nomeAzienda(): string {
    return (this._editForm.controls['ragioneSociale'].value ?? '').replace(/[^a-zA-Z0-9]/g, '');
  }

  public erroreCampo(campo: string): boolean {
    const ctrl = this._editForm.controls[campo];
    return ctrl.invalid && (ctrl.dirty || ctrl.touched);
  }

  public get passwordDiverse(): boolean {
    return this._editForm.hasError('passwordDiverse') && this._editForm.controls['conferma'].dirty;
  }

  private static lettereOCifre(control: AbstractControl): ValidationErrors | null {
    const valore: string = (control.value ?? '').replace(/[^a-zA-Z0-9]/g, '');
    return valore.length >= 3 ? null : { lettereOCifre: true };
  }

  private static passwordUguali(group: AbstractControl): ValidationErrors | null {
    return group.get('pw')?.value === group.get('conferma')?.value ? null : { passwordDiverse: true };
  }
}
