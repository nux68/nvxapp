import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../Utility/auth.service';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-logout-page',
  templateUrl: './logout-page.component.html',
  styleUrls: ['./logout-page.component.scss'],
  standalone: false
})
export class LogoutPageComponent  implements OnInit {

  public title!: string;
  logoutForm: FormGroup;

  constructor(private accountService: AccountService,
    private fb: FormBuilder,
    private navCtrl: NavController,
    private authService: AuthService
  ) {
    this.title = 'LogOut';

    this.logoutForm = this.fb.group({      
    });



  }

  ngOnInit() {

    

  }

  logout() {
    this.authService.UserName = null;
    this.navCtrl.navigateForward('/home');
  }

}
