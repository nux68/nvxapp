import { Injectable } from '@angular/core';
import { UserDataModel } from '../ClientServer-Service/Account/Models/user-load-model';
import { AuthService } from './auth.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { NavController } from '@ionic/angular';

@Injectable({
  providedIn: 'root'
})
export class UserNavigationService {

  constructor(private authService: AuthService,
              private navCtrl: NavController) { }

  private _userCronology: UserCronologyModel[] | null = [];

  private _userCronology$: BehaviorSubject<UserCronologyModel[] | null> = new BehaviorSubject<UserCronologyModel[]>(null);

  public get userCronology$(): Observable<UserCronologyModel[]> {
    return this._userCronology$.asObservable();
  }

  public get userCronology(): UserCronologyModel[] | null {
    return this._userCronology;
  }

  public UserPush(userData: UserDataModel, dataAdditional: UserDataAdditionalModel|null): void {

    

    const idx = this._userCronology.findIndex(x => x.userData.id == userData.id);
    if (idx == -1) {

      let usrCrono: UserCronologyModel = new UserCronologyModel();
      usrCrono.userData = userData;
      usrCrono.userDataAdditional = dataAdditional;

      this._userCronology.push(usrCrono);

      this.authService.setRole(userData.roles);
    }

    
  }

  public get UserCanGoBack(): boolean {
    return ( this._userCronology.length > 1 );
  }

  public UserGoBack(): void {

    if (this._userCronology.length > 1) {
      this._userCronology.pop();

      this.authService.setRole(this._userCronology[this._userCronology.length - 1].userData.roles);

      if(this._userCronology[this._userCronology.length - 1].userDataAdditional.gotoBackPage != null)
        this.navCtrl.navigateForward(this._userCronology[this._userCronology.length - 1].userDataAdditional.gotoBackPage);
    }

  }

  public UserGoTo(idUser: string): void {
    if (this._userCronology.length > 0) {

      var idx = this._userCronology.findIndex(x => x.userData.id == idUser);
      if (idx >= 0) {
        this._userCronology.splice(idx+1);

        this.authService.setRole(this._userCronology[this._userCronology.length - 1].userData.roles);

        if (this._userCronology[this._userCronology.length - 1].userDataAdditional.gotoBackPage != null)
          this.navCtrl.navigateForward(this._userCronology[this._userCronology.length - 1].userDataAdditional.gotoBackPage);
      }



    }
  }

  public LogOut(): void {
    this._userCronology = [];
    this.authService.LogOut();
  }

}

export class UserCronologyModel {

  public userData: UserDataModel;
  public userDataAdditional: UserDataAdditionalModel ;

}


// dati aggintivi che mi permettono di tornare alla situazione precedente in caso di back
export class UserDataAdditionalModel {

  public gotoBackPage: string | null;

}
