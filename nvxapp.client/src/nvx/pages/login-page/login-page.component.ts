import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserRolesInModel } from '../../ClientServer-Service/Account/Models/user-roles-model';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { LoginInModel } from '../../ClientServer-Service/Account/Models/login-model';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  standalone: false
})
export class LoginPageComponent  implements OnInit {

  public title!: string;
  loginForm: FormGroup;


  constructor(private accountService: AccountService,
              private fb: FormBuilder,
              private navCtrl: NavController,
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

      let request: GenericRequest<LoginInModel> = new GenericRequest<LoginInModel>();
      this.accountService.Login(request).subscribe(x => {

        var sucecss: boolean;
        sucecss = x.success;

        if (sucecss) {
          //console.log('Login successful', response);

          let request: GenericRequest<UserRolesInModel> = new GenericRequest<UserRolesInModel>();
          this.accountService.UserRoles(request).subscribe(x => {

            this.navCtrl.navigateForward('/home');

          });

        }
        else {

        }

      });


      //this.http.post('https://your-api.com/api/auth/login', formData).subscribe(
      //  (response) => {
      //    console.log('Login successful', response);
      //    this.navCtrl.navigateForward('/home');
      //  },
      //  (error) => {
      //    console.error('Login failed', error);
      //  }
      //);

    }
  }


}
