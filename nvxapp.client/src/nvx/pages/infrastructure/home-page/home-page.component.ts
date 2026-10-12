import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { NavController } from '@ionic/angular/lazy';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class HomePageComponent {

  public title!: string;

  constructor(public userNavigationService: UserNavigationService,
              private fabMenuService: FabMenuService,
              public navCtrl: NavController
  ) {
    this.title = 'Home';
  }

  ionViewWillEnter() {
  }

  // utente corrente (ultimo della cronologia di navigazione/impersonificazione); null se non autenticato
  public get userName(): string | null {
    const cronology = this.userNavigationService.userCronology;
    return cronology && cronology.length > 0 ? cronology[cronology.length - 1].userData?.userName ?? null : null;
  }


  openChat() {
    this.fabMenuService.openChat();
  }

  goToLogin() {
    this.navCtrl.navigateForward('/login');
  }

}
