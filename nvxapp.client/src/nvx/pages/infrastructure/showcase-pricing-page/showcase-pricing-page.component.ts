import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

// Showcase: piani e prezzi con le domande frequenti. Template: i testi sono in ShowcaseService.Contenuti.pricing
@Component({
  selector: 'app-showcase-pricing-page',
  templateUrl: './showcase-pricing-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcasePricingPageComponent {

  public readonly title = 'Prezzi';

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController) { }

  public get c(): ShowcaseContenuti['pricing'] {
    return this.showcaseService.Contenuti.pricing;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
