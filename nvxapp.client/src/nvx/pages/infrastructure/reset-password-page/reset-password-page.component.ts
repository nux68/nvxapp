import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PasswordRecoveryService } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/password-recovery.service';
import { PasswordRecoveryResetInModel } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/Models/password-recovery-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
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
export class ResetPasswordPageComponent extends BasePageConfirmCancelComponent<PasswordRecoveryResetInModel> {

  // stesso limite del server (PasswordRecoveryService)
  public readonly passwordMinLength = 8;

  public errori: string[] = [];
  public linkValido = true;
  // password cambiata: utente per il login
  public userNameCambiato: string | null = null;

  private btnAccedi: ButtonItem;
  private btnRichiedi: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private passwordRecoveryService: PasswordRecoveryService,
              public showcaseService: ShowcaseService,
              private route: ActivatedRoute,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnConfirm.text = this.transloco.translate('pages.resetPassword.savePassword');
    this.btnConfirm.image = 'checkmark-outline';
    this.btnConfirm.disabled = true;
    this.btnAccedi = userInterfaceService.Btn_LogIn;
    this.btnAccedi.text = this.transloco.translate('pages.resetPassword.signIn');
    this.btnAccedi.event = () => this.navCtrl.navigateRoot('/login', { state: { userName: this.userNameCambiato ?? '' } });
    this.btnRichiedi = new ButtonItem(this.transloco.translate('pages.resetPassword.newLink'), 'mail-outline', 'primary', false, () => this.navCtrl.navigateRoot('/forgot-password'));
  }

  get Title(): string { return this.transloco.translate('pages.resetPassword.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      pw: ['', [Validators.required, Validators.minLength(this.passwordMinLength)]],
      conferma: ['', [Validators.required]]
    }, { validators: ResetPasswordPageComponent.passwordUguali });
  }

  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateRoot('/login');
  }

  LoadData = (): Observable<PasswordRecoveryResetInModel> => {
    const model = new PasswordRecoveryResetInModel();
    model.userId = this.route.snapshot.queryParamMap.get('user') ?? '';
    model.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    this.linkValido = model.userId.length > 0 && model.token.length > 0;
    this.errori = this.linkValido ? [] : [this.transloco.translate('pages.resetPassword.invalidLink')];
    this.buttonbar = this.linkValido ? [this.btnConfirm, this.btnCancel] : [this.btnRichiedi];
    return of(model);
  };

  SaveData = (editModel: PasswordRecoveryResetInModel): Observable<boolean> => {
    this.errori = [];
    const request = new GenericRequest<PasswordRecoveryResetInModel>(PasswordRecoveryResetInModel);
    request.data.userId = editModel.userId;
    request.data.token = editModel.token;
    request.data.pw = editModel.pw;

    this.btnConfirm.disabled = true;
    return this.passwordRecoveryService.PasswordRecoveryReset(request).pipe(map(res => {
      this.btnConfirm.disabled = !this._editForm.valid;
      if (!res.success)
        return false; // errore inatteso: lo mostra l'interceptor
      // link scaduto o gia' usato, password non accettata: messaggi del server
      const messaggi = res.data?.messages ?? [];
      if (messaggi.length > 0) {
        this.errori = messaggi.map(m => m.text);
        this.buttonbar = [this.btnConfirm, this.btnRichiedi];
        return false;
      }
      this.userNameCambiato = res.data.userName;
      return true;
    }));
  };

  // password cambiata: si resta sulla pagina con l'esito e il pulsante per il login
  override AfterSave(saved: boolean): void {
    if (saved)
      this.buttonbar = [this.btnAccedi];
  }

  public get c(): ShowcaseContenuti['passwordRecovery'] {
    return this.showcaseService.Contenuti.passwordRecovery;
  }

  public erroreCampo(campo: string): boolean {
    const ctrl = this._editForm.controls[campo];
    return ctrl.invalid && (ctrl.dirty || ctrl.touched);
  }

  public get passwordDiverse(): boolean {
    return this._editForm.hasError('passwordDiverse') && this._editForm.controls['conferma'].dirty;
  }

  private static passwordUguali(group: AbstractControl): ValidationErrors | null {
    return group.get('pw')?.value === group.get('conferma')?.value ? null : { passwordDiverse: true };
  }
}
