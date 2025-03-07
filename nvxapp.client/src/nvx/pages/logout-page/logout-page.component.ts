import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { UserNavigationService } from '../../Utility/user-navigation.service';

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
              private userNavigationService: UserNavigationService
  ) {
    this.title = 'Logout';

    this.logoutForm = this.fb.group({      
    });

  }


  ionViewWillEnter() {
  }

  ngOnInit() {

  }

  logout() {
    this.userNavigationService.LogOut();
    this.navCtrl.navigateForward('/home');
  }

}
