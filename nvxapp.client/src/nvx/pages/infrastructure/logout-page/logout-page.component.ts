import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../../ClientServer-Service/Account/account.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-logout-page',
  templateUrl: './logout-page.component.html',
  styleUrls: ['./logout-page.component.scss'],
  standalone: false
})
export class LogoutPageComponent  implements OnInit {

  public title!: string;
  logoutForm: FormGroup;
  public buttonbar: ButtonItem[] = [];

  constructor(private accountService: AccountService,
              private fb: FormBuilder,
              protected userInterfaceService: UserInterfaceService,
              private navCtrl: NavController,
              private userNavigationService: UserNavigationService
  ) {
    this.title = 'Logout';

    this.buttonbar = userInterfaceService.Btn_LogOutAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;

    this.logoutForm = this.fb.group({      
    });

  }


  ionViewWillEnter() {
  }

  ngOnInit() {
    this.logoutForm.statusChanges.subscribe(() => {
      this.buttonbar[0].disabled = !this.logoutForm.valid;
    });
  }

  logout() {
    this.userNavigationService.LogOut();
    this.navCtrl.navigateForward('/home');
  }

  private _handleButtonConfirmClick = (param: object) => {

    this.logout()

  }

  private _handleButtonCancelClick = (param: object) => {
    this.navCtrl.navigateForward('/home');
  }

}
