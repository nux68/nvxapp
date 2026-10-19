import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

// Showcase: presentazione e valori. Template: i testi sono in ShowcaseService.Contenuti.about
@Component({
  selector: 'app-showcase-about-page',
  templateUrl: './showcase-about-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseAboutPageComponent {

  public readonly title = 'Chi siamo';

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController) { }

  public get c(): ShowcaseContenuti['about'] {
    return this.showcaseService.Contenuti.about;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
