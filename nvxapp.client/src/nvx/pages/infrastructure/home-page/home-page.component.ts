import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Platform } from '@ionic/angular';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
  standalone: false
})
export class HomePageComponent  implements OnInit {

  public title!: string;

  // Flag per gestire la visualizzazione e l'animazione
  public showSplash = false;
  public startFadeOut = false;
  // Variabile statica per tracciare la prima visita
  private static hasVisited = false;

  constructor(public userNavigationService: UserNavigationService,
              private platform: Platform,
  ) {
    this.title = 'Home';
  }

  ionViewWillEnter() {
  }
  
  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  ngOnInit() {
    if (!HomePageComponent.hasVisited) {
      this.showSplash = true;
      HomePageComponent.hasVisited = true;

      // Avvia la dissolvenza dopo 2 secondi (l'animazione dura 1s, totale 3s)
      setTimeout(() => {
        this.startFadeOut = true;
      }, 10000);

      // Nasconde completamente l'elemento dopo 3 secondi
      setTimeout(() => {
        this.showSplash = false;
      }, 50000);
    }
  }

}
