import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavController } from '@ionic/angular';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RegistrationService } from '../../../ClientServer-Service/Infrastructure/Registration/registration.service';
import { CompanyRegisterConfirmInModel, CompanyRegisterUserModel } from '../../../ClientServer-Service/Infrastructure/Registration/Models/company-register-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { TranslocoService } from '@jsverse/transloco';

/*
 Showcase: conferma della registrazione dal link ricevuto per email (/confirm-email?token=...).
 Il server crea l'azienda con i suoi utenti; si mostrano gli utenti creati e si va al login con il
 primo gia' compilato. Aprire di nuovo il link mostra la stessa azienda (non ne crea un'altra).
*/
@Component({
  selector: 'app-confirm-email-page',
  templateUrl: './confirm-email-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ConfirmEmailPageComponent {

  public readonly title: string;

  public inCorso = false;
  public errori: string[] = [];
  public descrizione = '';
  public utentiCreati: CompanyRegisterUserModel[] | null = null;

  public buttonbar: ButtonItem[] = [];
  private btnAccedi: ButtonItem;
  private btnRegistrati: ButtonItem;

  constructor(private registrationService: RegistrationService,
              public showcaseService: ShowcaseService,
              userInterfaceService: UserInterfaceService,
              private route: ActivatedRoute,
              private navCtrl: NavController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('pages.confirmEmail.title');
    this.btnAccedi = userInterfaceService.Btn_LogIn;
    this.btnAccedi.text = this.transloco.translate('pages.confirmEmail.signIn');
    this.btnAccedi.event = () => this.accedi();
    this.btnRegistrati = new ButtonItem(this.transloco.translate('pages.confirmEmail.signUp'), 'person-add-outline', 'primary', false, () => this.navCtrl.navigateRoot('/register'));
  }

  ionViewWillEnter() {
    this.conferma(this.route.snapshot.queryParamMap.get('token'));
  }

  public get c(): ShowcaseContenuti['register'] {
    return this.showcaseService.Contenuti.register;
  }

  private conferma(token: string | null): void {
    this.errori = [];
    this.utentiCreati = null;
    this.buttonbar = [];
    if (!token) {
      this.errori = [this.transloco.translate('pages.confirmEmail.invalidLink')];
      this.buttonbar = [this.btnRegistrati];
      return;
    }

    const request = new GenericRequest<CompanyRegisterConfirmInModel>(CompanyRegisterConfirmInModel);
    request.data.token = token;
    this.inCorso = true;
    this.registrationService.CompanyRegisterConfirm(request).subscribe({
      next: res => {
        this.inCorso = false;
        if (!res.success)
          return; // errore inatteso: lo mostra l'interceptor
        // link scaduto, gia' usato o nome preso nel frattempo: messaggi del server
        const messaggi = res.data?.messages ?? [];
        if (messaggi.length > 0) {
          this.errori = messaggi.map(m => m.text);
          this.buttonbar = [this.btnRegistrati];
          return;
        }
        this.descrizione = res.data.descrizione;
        this.utentiCreati = res.data.users;
        this.buttonbar = [this.btnAccedi];
      },
      error: () => this.inCorso = false
    });
  }

  private accedi(): void {
    this.navCtrl.navigateRoot('/login', { state: { userName: this.utentiCreati?.[0]?.userName ?? '' } });
  }
}
