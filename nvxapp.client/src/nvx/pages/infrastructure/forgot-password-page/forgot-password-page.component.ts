import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PasswordRecoveryService } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/password-recovery.service';
import { PasswordRecoveryRequestInModel } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/Models/password-recovery-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';

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
export class ForgotPasswordPageComponent extends BasePageConfirmCancelComponent<PasswordRecoveryRequestInModel> {

  public errori: string[] = [];
  // richiesta inviata: validita' del link in ore
  public inviata = false;
  public scadenzaOre = 0;

  private btnLogin: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private passwordRecoveryService: PasswordRecoveryService,
              public showcaseService: ShowcaseService,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnConfirm.text = this.transloco.translate('pages.forgotPassword.sendLink');
    this.btnConfirm.image = 'mail-outline';
    this.btnConfirm.disabled = true;
    this.btnLogin = userInterfaceService.Btn_LogIn;
    this.btnLogin.text = this.transloco.translate('pages.forgotPassword.signIn');
    this.btnLogin.event = () => this.navCtrl.navigateRoot('/login');
  }

  get Title(): string { return this.transloco.translate('pages.forgotPassword.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      userName: ['', [Validators.required]]
    });
  }

  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateRoot('/login');
  }

  LoadData = (): Observable<PasswordRecoveryRequestInModel> => {
    const model = new PasswordRecoveryRequestInModel();
    // arrivo dal login: nome utente gia' scritto (rientrando dopo l'invio resta l'esito)
    if (!this.inviata)
      model.userName = history.state?.userName || this._editForm.controls['userName'].value || null;
    return of(model);
  };

  SaveData = (editModel: PasswordRecoveryRequestInModel): Observable<boolean> => {
    this.errori = [];
    const request = new GenericRequest<PasswordRecoveryRequestInModel>(PasswordRecoveryRequestInModel);
    request.data.userName = editModel.userName;

    this.btnConfirm.disabled = true;
    return this.passwordRecoveryService.PasswordRecoveryRequest(request).pipe(map(res => {
      this.btnConfirm.disabled = !this._editForm.valid;
      if (!res.success)
        return false; // errore inatteso: lo mostra l'interceptor
      const messaggi = res.data?.messages ?? [];
      if (messaggi.length > 0) {
        this.errori = messaggi.map(m => m.text);
        return false;
      }
      this.inviata = true;
      this.scadenzaOre = res.data.scadenzaOre;
      return true;
    }));
  };

  // richiesta inviata: si resta sulla pagina con l'esito
  override AfterSave(saved: boolean): void {
    if (saved)
      this.buttonbar = [this.btnLogin];
  }

  public get c(): ShowcaseContenuti['passwordRecovery'] {
    return this.showcaseService.Contenuti.passwordRecovery;
  }
}
