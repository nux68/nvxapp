import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { NavController } from '@ionic/angular';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti, ShowcaseVoce } from '../../../Utility/infrastructure/showcase.model';

// Showcase: funzionalita' del prodotto. Template: i testi sono in ShowcaseService.Contenuti.features.
// Le voci con il dettaglio si aprono in un dialog minimale (ion-modal inline, senza pulsanti conferma/annulla).
@Component({
  selector: 'app-showcase-features-page',
  templateUrl: './showcase-features-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseFeaturesPageComponent {

  public readonly title: string;

  /** voce mostrata nel dialog di dettaglio (null = dialog chiuso) */
  public dettaglio: ShowcaseVoce | null = null;

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

  public apriDettaglio(voce: ShowcaseVoce): void {
    this.dettaglio = voce;
  }

  public chiudiDettaglio(): void {
    this.dettaglio = null;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }
}
