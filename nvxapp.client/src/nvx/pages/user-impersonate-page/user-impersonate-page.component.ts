import { Component, OnInit } from '@angular/core';
import { UserCronologyModel, UserNavigationService } from '../../Utility/user-navigation.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-user-impersonate-page',
  templateUrl: './user-impersonate-page.component.html',
  styleUrls: ['./user-impersonate-page.component.scss'],
  standalone: false
})
export class UserImpersonatePageComponent  implements OnInit {

  public title!: string;
  public selectedValue: string|null = null;

  public userCronology: UserCronologyModel[] | null = null;

  constructor(public userNavigationService: UserNavigationService,
              private navCtrl: NavController,
              private fb: FormBuilder,) {

    this.title = 'UserImpersonate';
  }

  ngOnInit() {
      this.userCronology = this.userNavigationService.userCronology;
      this.selectedValue = this.userCronology[this.userCronology.length - 1].userData.id;
  }

  UserGoBack() {
    this.userNavigationService.UserGoTo(this.selectedValue);
  }

  LogOut() {
    this.userNavigationService.LogOut();
    this.navCtrl.navigateForward('/home');
  }

  

}
