import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, map, of, switchMap } from 'rxjs';
import { LoginInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/login-model';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { UserLoadInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserDataAdditionalModel, UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { environment } from '../../../../environments/environment';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { SHOWCASE_AFTER_LOGIN } from '../../RoleGuard/infrastructure/showcase-guard';
import { SITE_NAME } from '../../../Utility/infrastructure/site-name';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LoginPageComponent extends BasePageConfirmCancelComponent<LoginInModel> {

  public readonly siteName = SITE_NAME;
  public loginError!: string;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private accountService: AccountService,
              private userNavigationService: UserNavigationService,
              private authService: AuthService,
              private signalrService: SignalrService,
              private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);

    const btnLogIn = userInterfaceService.Btn_LogIn;
    this.btnConfirm.text = btnLogIn.text;
    this.btnConfirm.image = btnLogIn.image;
  }

  get Title(): string { return this.transloco.translate('pages.login.title'); }

  get EditForm(): FormGroup {

    //return this.fb.group({
    //  userName: [environment.production ? '' : 'max_poweradmin', [Validators.required, Validators.minLength(3)]],
    //  password: [environment.production ? '' : 'max', [Validators.required, Validators.minLength(3)]]
    //});

    return this.fb.group({
      userName: ['CompanyPowerAdmin_1_1_1', [Validators.required, Validators.minLength(3)]],
      password: ['1234', [Validators.required, Validators.minLength(3)]]

      // userName: [ 'superuser', [Validators.required, Validators.minLength(3)]],
      // password: [ '1234', [Validators.required, Validators.minLength(3)]]
    });
  }

  override ButtonCancelClickEv = (param: object) => {
    this.navCtrl.navigateForward('/home');
  }

  LoadData = (): Observable<LoginInModel> => {
    // resta quanto gia' scritto; arrivo dalla registrazione: utente appena creato gia' compilato, password da inserire
    const model: LoginInModel = Object.assign(new LoginInModel(), this._editForm.value);
    const userName = history.state?.userName;
    if (userName) {
      model.userName = userName;
      model.password = '';
    }
    return of(model);
  };

  SaveData = (editModel: LoginInModel): Observable<boolean> => {
    const request = new GenericRequest<LoginInModel>(LoginInModel);
    request.data.userName = editModel.userName;
    request.data.password = editModel.password;

    return this.accountService.Login(request).pipe(switchMap(x => {

      if (!x.success || x.data.messages.length > 0) {
        this.loginError = x.success ? x.data.messages[0].text : x.messages[0].text;
        return of(false);
      }

      this.authService.Token = x.data.token;

      const requestLoad = new GenericRequest<UserLoadInModel>(UserLoadInModel);
      requestLoad.data.id = x.data.id;
      return this.accountService.UserLoad(requestLoad).pipe(map(usl => {
        if (!usl.success)
          return false;

        if (environment.signalR.useSignalR) {
          this.signalrService.startConnection();
        }

        const userDataAdditional: UserDataAdditionalModel = new UserDataAdditionalModel();
        userDataAdditional.gotoBackPage = SHOWCASE_AFTER_LOGIN;
        this.userNavigationService.UserPush(usl.data.userData, userDataAdditional);
        return true;
      }));
    }));
  };

  // entrati: si va alla pagina iniziale; con un errore si resta sul login (messaggio sotto i campi)
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.navigateForward(SHOWCASE_AFTER_LOGIN);
  }

  // recupero dell'accesso: nome utente gia' scritto, se c'e'
  passwordDimenticata() {
    this.navCtrl.navigateRoot('/forgot-password', { state: { userName: this._editForm.controls['userName'].value ?? '' } });
  }

}
