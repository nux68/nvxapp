import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NavController } from '@ionic/angular';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PasswordRecoveryService } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/password-recovery.service';
import { PasswordRecoveryResetInModel } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/Models/password-recovery-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { TranslocoService } from '@jsverse/transloco';

/*
 Showcase: nuova password dal link ricevuto per email (/reset-password?user=...&token=...).
 Fatto il cambio si va al login con l'utente gia' compilato.
*/
@Component({
  selector: 'app-reset-password-page',
  templateUrl: './reset-password-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ResetPasswordPageComponent implements OnInit {

  public readonly title: string;

  // stesso limite del server (PasswordRecoveryService)
  public readonly passwordMinLength = 8;

  public form: FormGroup;
  public errori: string[] = [];
  public linkValido = true;
  // password cambiata: utente per il login
  public userNameCambiato: string | null = null;

  public buttonbar: ButtonItem[] = [];
  private btnSalva: ButtonItem;
  private btnAccedi: ButtonItem;
  private btnRichiedi: ButtonItem;

  private userId = '';
  private token = '';

  constructor(private passwordRecoveryService: PasswordRecoveryService,
              public showcaseService: ShowcaseService,
              userInterfaceService: UserInterfaceService,
              private fb: FormBuilder,
              private route: ActivatedRoute,
              private navCtrl: NavController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('pages.resetPassword.title');
    this.btnSalva = new ButtonItem(this.transloco.translate('pages.resetPassword.savePassword'), 'checkmark-outline', 'primary', true, () => this.salva());
    this.btnAccedi = userInterfaceService.Btn_LogIn;
    this.btnAccedi.text = this.transloco.translate('pages.resetPassword.signIn');
    this.btnAccedi.event = () => this.navCtrl.navigateRoot('/login', { state: { userName: this.userNameCambiato ?? '' } });
    this.btnRichiedi = new ButtonItem(this.transloco.translate('pages.resetPassword.newLink'), 'mail-outline', 'primary', false, () => this.navCtrl.navigateRoot('/forgot-password'));

    this.form = this.fb.group({
      pw: ['', [Validators.required, Validators.minLength(this.passwordMinLength)]],
      conferma: ['', [Validators.required]]
    }, { validators: ResetPasswordPageComponent.passwordUguali });
  }

  ngOnInit() {
    this.form.statusChanges.subscribe(() => {
      this.btnSalva.disabled = !this.form.valid;
    });
  }

  ionViewWillEnter() {
    this.userId = this.route.snapshot.queryParamMap.get('user') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.linkValido = this.userId.length > 0 && this.token.length > 0;
    this.errori = this.linkValido ? [] : [this.transloco.translate('pages.resetPassword.invalidLink')];
    this.buttonbar = this.linkValido ? [this.btnSalva] : [this.btnRichiedi];
  }

  public get c(): ShowcaseContenuti['passwordRecovery'] {
    return this.showcaseService.Contenuti.passwordRecovery;
  }

  public erroreCampo(campo: string): boolean {
    const ctrl = this.form.controls[campo];
    return ctrl.invalid && (ctrl.dirty || ctrl.touched);
  }

  public get passwordDiverse(): boolean {
    return this.form.hasError('passwordDiverse') && this.form.controls['conferma'].dirty;
  }

  private salva(): void {
    if (!this.form.valid)
      return;

    this.errori = [];
    const request = new GenericRequest<PasswordRecoveryResetInModel>(PasswordRecoveryResetInModel);
    request.data.userId = this.userId;
    request.data.token = this.token;
    request.data.pw = this.form.controls['pw'].value;

    this.btnSalva.disabled = true;
    this.passwordRecoveryService.PasswordRecoveryReset(request).subscribe({
      next: res => {
        this.btnSalva.disabled = !this.form.valid;
        if (!res.success)
          return; // errore inatteso: lo mostra l'interceptor
        // link scaduto o gia' usato, password non accettata: messaggi del server
        const messaggi = res.data?.messages ?? [];
        if (messaggi.length > 0) {
          this.errori = messaggi.map(m => m.text);
          this.buttonbar = [this.btnSalva, this.btnRichiedi];
          return;
        }
        this.userNameCambiato = res.data.userName;
        this.buttonbar = [this.btnAccedi];
      },
      error: () => this.btnSalva.disabled = !this.form.valid
    });
  }

  private static passwordUguali(group: AbstractControl): ValidationErrors | null {
    return group.get('pw')?.value === group.get('conferma')?.value ? null : { passwordDiverse: true };
  }
}
