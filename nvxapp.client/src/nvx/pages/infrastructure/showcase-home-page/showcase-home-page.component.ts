import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { NavController } from '@ionic/angular';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { SITE_NAME } from '../../../Utility/infrastructure/site-name';
import { ASSISTENTI_AI, ShowcaseContenuti, ShowcaseVoce, voceShowcase } from '../../../Utility/infrastructure/showcase.model';

// Showcase: pagina iniziale (solo prima del login). Template: i testi sono in ShowcaseService.Contenuti.home
@Component({
  selector: 'app-showcase-home-page',
  templateUrl: './showcase-home-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseHomePageComponent {

  public readonly title: string;

  constructor(public showcaseService: ShowcaseService,
              private navCtrl: NavController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('showcase.menu.home');
  }

  public get c(): ShowcaseContenuti['home'] {
    return this.showcaseService.Contenuti.home;
  }

  public vai(url: string): void {
    this.navCtrl.navigateRoot(url);
  }

  // ---------------------------------------------------------------- integrazione AI
  // Sezione uguale in tutti i rami (non e' in Contenuti.home, che i rami ridefiniscono): testi in showcase.ai.
  // Per ora solo presentazione: l'integrazione e' prevista nei piani Pro (vedi pagina prezzi).

  public readonly assistentiAi = ASSISTENTI_AI;
  public readonly sito = { siteName: SITE_NAME };

  public get puntiAi(): ShowcaseVoce[] {
    return [
      voceShowcase(this.transloco, 'chatbubbles-outline', 'showcase.ai.punti.domande'),
      voceShowcase(this.transloco, 'flash-outline', 'showcase.ai.punti.operazioni'),
      voceShowcase(this.transloco, 'document-text-outline', 'showcase.ai.punti.report'),
      voceShowcase(this.transloco, 'shield-checkmark-outline', 'showcase.ai.punti.sicurezza')
    ];
  }

  public get esempiAi(): string[] {
    const esempi = this.transloco.translateObject<unknown>('showcase.ai.esempi');
    return Array.isArray(esempi) ? esempi.filter((e): e is string => typeof e === 'string') : [];
  }

  public vaiAllaSezioneAi(): void {
    document.getElementById('nvx-sc-ai')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

