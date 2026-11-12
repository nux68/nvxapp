import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
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

  public readonly title: string;

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('showcase.menu.features');
  }

  public get c(): ShowcaseContenuti['features'] {
    return this.showcaseService.Contenuti.features;
  }

  /** indice dei gruppi: porta al blocco del gruppo scelto */
  public vaiAlGruppo(indice: number): void {
    document.getElementById('nvx-sc-gruppo-' + indice)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
