import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserRolesInModel } from '../../ClientServer-Service/Account/Models/user-roles-model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { LoginInModel } from '../../ClientServer-Service/Account/Models/login-model';
import { AuthService } from '../../Utility/auth.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  standalone: false
})
export class LoginPageComponent  implements OnInit {

  public title!: string;
  loginForm: FormGroup;
  public loginError!: string;

  constructor(private accountService: AccountService,
              private fb: FormBuilder,
              private navCtrl: NavController,
              private authService: AuthService
  ) {
    this.title = 'Login';

    this.loginForm = this.fb.group({
      //email: ['', [Validators.required, Validators.email]],
      email: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(3)]]
    });
    

  }

  ngOnInit() {



  }

  login() {
    if (this.loginForm.valid) {
      const formData = this.loginForm.value;

      const UserName = this.loginForm.controls['email'].value;
      const Password = this.loginForm.controls['password'].value;

      let request: GenericRequest<LoginInModel> = new GenericRequest<LoginInModel>(LoginInModel);
      request.data.userName = UserName;
      request.data.password = Password;
      this.accountService.Login(request).subscribe(x => {

        var sucecss: boolean;
        sucecss = x.success;

        if (sucecss && x.data.messages.length == 0) {
          
          this.authService.UserName = UserName;

          let request: GenericRequest<UserRolesInModel> = new GenericRequest<UserRolesInModel>(UserRolesInModel);
          this.accountService.UserRoles(request).subscribe(x => {

            this.navCtrl.navigateForward('/home');

          });

        }
        else {
          this.loginError = x.data.messages[0].text;  //x.messages[0].text;
          this.authService.UserName = null;
        }

      });

    }
  }


}
