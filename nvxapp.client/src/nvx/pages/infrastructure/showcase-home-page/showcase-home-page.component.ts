import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

// Showcase: pagina iniziale (solo prima del login). Template: i testi sono in ShowcaseService.Contenuti.home
@Component({
  selector: 'app-showcase-home-page',
  templateUrl: './showcase-home-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseHomePageComponent {

  public readonly title = 'Home';

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController) { }

  public get c(): ShowcaseContenuti['home'] {
    return this.showcaseService.Contenuti.home;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
