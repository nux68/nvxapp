import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

// Showcase: funzionalita' del prodotto. Template: i testi sono in ShowcaseService.Contenuti.features
@Component({
  selector: 'app-showcase-features-page',
  templateUrl: './showcase-features-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseFeaturesPageComponent {

  public readonly title = 'Funzionalità';

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController) { }

  public get c(): ShowcaseContenuti['features'] {
    return this.showcaseService.Contenuti.features;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
