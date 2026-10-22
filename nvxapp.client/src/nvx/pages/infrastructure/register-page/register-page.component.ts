import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RegistrationService } from '../../../ClientServer-Service/Infrastructure/Registration/registration.service';
import { CompanyRegisterInModel } from '../../../ClientServer-Service/Infrastructure/Registration/Models/company-register-model';
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

  public errori: string[] = [];
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
      accettaCondizioni: [false, [Validators.requiredTrue]]
    }, { validators: RegisterPageComponent.passwordUguali });
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
