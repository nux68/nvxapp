import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PasswordRecoveryService } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/password-recovery.service';
import { PasswordChangeInModel } from '../../../ClientServer-Service/Infrastructure/PasswordRecovery/Models/password-recovery-model';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';

/*
 Showcase: cambio della password conoscendo quella attuale (/change-password, dal login), senza email.
 Si apre con il nome utente gia' scritto nel login; fatto il cambio si torna al login con l'utente compilato.
*/
@Component({
  selector: 'app-change-password-page',
  templateUrl: './change-password-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ChangePasswordPageComponent extends BasePageConfirmCancelComponent<PasswordChangeInModel> {

  // stesso limite del server (PasswordRecoveryService)
  public readonly passwordMinLength = 8;

  public errori: string[] = [];
  // password cambiata: utente per il login
  public userNameCambiato: string | null = null;

  private btnAccedi: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private passwordRecoveryService: PasswordRecoveryService,
              public showcaseService: ShowcaseService,
              private transloco: TranslocoService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnConfirm.text = this.transloco.translate('pages.changePassword.savePassword');
    this.btnConfirm.image = 'checkmark-outline';
    this.btnConfirm.disabled = true;
    this.btnAccedi = userInterfaceService.Btn_LogIn;
    this.btnAccedi.text = this.transloco.translate('pages.changePassword.signIn');
    this.btnAccedi.event = () => this.navCtrl.navigateRoot('/login', { state: { userName: this.userNameCambiato ?? '' } });
  }

  get Title(): string { return this.transloco.translate('pages.changePassword.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      userName: ['', [Validators.required]],
      pw: ['', [Validators.required]],
      newPw: ['', [Validators.required, Validators.minLength(this.passwordMinLength)]],
      conferma: ['', [Validators.required]]
    }, { validators: ChangePasswordPageComponent.passwordUguali });
  }

  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateRoot('/login');
  }

  // dal login: nome utente gia' scritto, se c'e'
  LoadData = (): Observable<PasswordChangeInModel> => {
    const model = new PasswordChangeInModel();
    model.userName = history.state?.userName ?? '';
    return of(model);
  };

  SaveData = (editModel: PasswordChangeInModel): Observable<boolean> => {
    this.errori = [];
    const request = new GenericRequest<PasswordChangeInModel>(PasswordChangeInModel);
    request.data.userName = editModel.userName;
    request.data.pw = editModel.pw;
    request.data.newPw = editModel.newPw;

    this.btnConfirm.disabled = true;
    return this.passwordRecoveryService.PasswordChange(request).pipe(map(res => {
      this.btnConfirm.disabled = !this._editForm.valid;
      if (!res.success)
        return false; // errore inatteso: lo mostra l'interceptor
      // utente o password attuale errati, nuova password non accettata: messaggi del server
      const messaggi = res.data?.messages ?? [];
      if (messaggi.length > 0) {
        this.errori = messaggi.map(m => m.text);
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
    return group.get('newPw')?.value === group.get('conferma')?.value ? null : { passwordDiverse: true };
  }
}
