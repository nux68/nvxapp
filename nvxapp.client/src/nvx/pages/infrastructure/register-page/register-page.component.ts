import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular/lazy';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RegistrationService } from '../../../ClientServer-Service/Infrastructure/Registration/registration.service';
import { CompanyRegisterInModel } from '../../../ClientServer-Service/Infrastructure/Registration/Models/company-register-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

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
export class RegisterPageComponent implements OnInit {

  public readonly title = 'Registrazione';

  // stessi limiti del server (RegistrationService)
  public readonly passwordMinLength = 8;
  public readonly ragioneSocialeMaxLength = 40;

  public form: FormGroup;
  public errori: string[] = [];
  // richiesta inviata: indirizzo a cui e' arrivato il link e sua validita'
  public mailInviata: string | null = null;
  public scadenzaOre = 0;

  public buttonbar: ButtonItem[] = [];
  private btnRegistrati: ButtonItem;
  private btnAnnulla: ButtonItem;
  private btnHome: ButtonItem;

  constructor(private registrationService: RegistrationService,
              public showcaseService: ShowcaseService,
              userInterfaceService: UserInterfaceService,
              private fb: FormBuilder,
              private navCtrl: NavController) {

    this.btnRegistrati = new ButtonItem('Registrati', 'person-add-outline', 'primary', true, () => this.registra());
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = () => this.navCtrl.navigateRoot('/home');
    this.btnHome = new ButtonItem('Home', 'home-outline', 'primary', false, () => this.navCtrl.navigateRoot('/home'));
    this.buttonbar = [this.btnRegistrati, this.btnAnnulla];

    this.form = this.fb.group({
      ragioneSociale: ['', [Validators.required, Validators.maxLength(this.ragioneSocialeMaxLength), RegisterPageComponent.lettereOCifre]],
      mail: ['', [Validators.required, Validators.email]],
      pw: ['', [Validators.required, Validators.minLength(this.passwordMinLength)]],
      conferma: ['', [Validators.required]],
      accettaCondizioni: [false, [Validators.requiredTrue]]
    }, { validators: RegisterPageComponent.passwordUguali });
  }

  ngOnInit() {
    this.form.statusChanges.subscribe(() => {
      this.btnRegistrati.disabled = !this.form.valid;
    });
  }

  public get c(): ShowcaseContenuti['register'] {
    return this.showcaseService.Contenuti.register;
  }

  /** nome con cui nasce l'azienda: il server tiene solo lettere e cifre (anche per i nomi degli utenti) */
  public get nomeAzienda(): string {
    return (this.form.controls['ragioneSociale'].value ?? '').replace(/[^a-zA-Z0-9]/g, '');
  }

  public erroreCampo(campo: string): boolean {
    const ctrl = this.form.controls[campo];
    return ctrl.invalid && (ctrl.dirty || ctrl.touched);
  }

  public get passwordDiverse(): boolean {
    return this.form.hasError('passwordDiverse') && this.form.controls['conferma'].dirty;
  }

  private registra(): void {
    if (!this.form.valid)
      return;

    this.errori = [];
    const request = new GenericRequest<CompanyRegisterInModel>(CompanyRegisterInModel);
    request.data.ragioneSociale = this.form.controls['ragioneSociale'].value;
    request.data.mail = this.form.controls['mail'].value;
    request.data.pw = this.form.controls['pw'].value;
    request.data.accettaCondizioni = this.form.controls['accettaCondizioni'].value === true;

    this.btnRegistrati.disabled = true;
    this.registrationService.CompanyRegister(request).subscribe({
      next: res => {
        this.btnRegistrati.disabled = !this.form.valid;
        if (!res.success)
          return; // errore inatteso: lo mostra l'interceptor
        // dati non validi o nome gia' usato: messaggi del server
        const messaggi = res.data?.messages ?? [];
        if (messaggi.length > 0) {
          this.errori = messaggi.map(m => m.text);
          return;
        }
        this.mailInviata = res.data.mail;
        this.scadenzaOre = res.data.scadenzaOre;
        this.buttonbar = [this.btnHome];
      },
      error: () => this.btnRegistrati.disabled = !this.form.valid
    });
  }

  private static lettereOCifre(control: AbstractControl): ValidationErrors | null {
    const valore: string = (control.value ?? '').replace(/[^a-zA-Z0-9]/g, '');
    return valore.length >= 3 ? null : { lettereOCifre: true };
  }

  private static passwordUguali(group: AbstractControl): ValidationErrors | null {
    return group.get('pw')?.value === group.get('conferma')?.value ? null : { passwordDiverse: true };
  }
}
