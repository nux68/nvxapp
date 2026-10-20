import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular/lazy';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PasswordRecoveryService } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/password-recovery.service';
import { PasswordRecoveryRequestInModel } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/Models/password-recovery-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

/*
 Showcase: password dimenticata (prima del login). Dal nome utente il server invia all'email
 dell'utente il link per sceglierne una nuova (pagina /reset-password, ResetPasswordPageComponent).
 La risposta e' sempre la stessa, che l'utente esista o no.
*/
@Component({
  selector: 'app-forgot-password-page',
  templateUrl: './forgot-password-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ForgotPasswordPageComponent implements OnInit {

  public readonly title = 'Password dimenticata';

  public form: FormGroup;
  public errori: string[] = [];
  // richiesta inviata: validita' del link in ore
  public inviata = false;
  public scadenzaOre = 0;

  public buttonbar: ButtonItem[] = [];
  private btnInvia: ButtonItem;
  private btnAnnulla: ButtonItem;
  private btnLogin: ButtonItem;

  constructor(private passwordRecoveryService: PasswordRecoveryService,
              public showcaseService: ShowcaseService,
              userInterfaceService: UserInterfaceService,
              private fb: FormBuilder,
              private navCtrl: NavController) {
    this.btnInvia = new ButtonItem('Invia link', 'mail-outline', 'primary', true, () => this.invia());
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = () => this.navCtrl.navigateRoot('/login');
    this.btnLogin = userInterfaceService.Btn_LogIn;
    this.btnLogin.text = 'Accedi';
    this.btnLogin.event = () => this.navCtrl.navigateRoot('/login');
    this.buttonbar = [this.btnInvia, this.btnAnnulla];

    this.form = this.fb.group({
      userName: ['', [Validators.required]]
    });
  }

  ngOnInit() {
    this.form.statusChanges.subscribe(() => {
      this.btnInvia.disabled = !this.form.valid;
    });
  }

  ionViewWillEnter() {
    // arrivo dal login: nome utente gia' scritto
    const userName = history.state?.userName;
    if (userName && !this.inviata)
      this.form.controls['userName'].setValue(userName);
  }

  public get c(): ShowcaseContenuti['passwordRecovery'] {
    return this.showcaseService.Contenuti.passwordRecovery;
  }

  private invia(): void {
    if (!this.form.valid)
      return;

    this.errori = [];
    const request = new GenericRequest<PasswordRecoveryRequestInModel>(PasswordRecoveryRequestInModel);
    request.data.userName = this.form.controls['userName'].value;

    this.btnInvia.disabled = true;
    this.passwordRecoveryService.PasswordRecoveryRequest(request).subscribe({
      next: res => {
        this.btnInvia.disabled = !this.form.valid;
        if (!res.success)
          return; // errore inatteso: lo mostra l'interceptor
        const messaggi = res.data?.messages ?? [];
        if (messaggi.length > 0) {
          this.errori = messaggi.map(m => m.text);
          return;
        }
        this.inviata = true;
        this.scadenzaOre = res.data.scadenzaOre;
        this.buttonbar = [this.btnLogin];
      },
      error: () => this.btnInvia.disabled = !this.form.valid
    });
  }
}
