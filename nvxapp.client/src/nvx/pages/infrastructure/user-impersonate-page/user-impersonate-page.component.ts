import { Component, OnInit } from '@angular/core';
import { UserCronologyModel, UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

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

  public buttonbarGoTo:  ButtonItem;
  public buttonbarLogOut: ButtonItem;
  public buttonbar: ButtonItem[] = [];

  constructor(public userNavigationService: UserNavigationService,
              private navCtrl: NavController,
              protected userInterfaceService: UserInterfaceService,
              private fb: FormBuilder,) {

    this.title = 'UserImpersonate';

    this.buttonbarGoTo = userInterfaceService.Btn_Impersona;
    this.buttonbarGoTo.event = this._handleUserGoBackClick;
    this.buttonbar.push(this.buttonbarGoTo);

    this.buttonbarLogOut = userInterfaceService.Btn_LogOut;
    this.buttonbarLogOut.event = this._handleLogOutClick;
    this.buttonbar.push(this.buttonbarLogOut);



  }

  ionViewWillEnter() {
    this.userCronology = this.userNavigationService.userCronology;
    this.selectedValue = this.userCronology[this.userCronology.length - 1].userData.id;
  }

  ngOnInit() {}

  UserGoBack() {
    this.userNavigationService.UserGoTo(this.selectedValue);
  }

  LogOut() {
    this.userNavigationService.LogOut();
    this.navCtrl.navigateForward('/home');
  }

  private _handleLogOutClick = (param: object) => {

    this.LogOut()

  }
  private _handleUserGoBackClick = (param: object) => {

    this.UserGoBack()

  }

}
