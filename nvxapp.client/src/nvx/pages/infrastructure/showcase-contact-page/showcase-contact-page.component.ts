import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ShowcaseService } from '../../../Utility/infrastructure/showcase.service';
import { ShowcaseContenuti } from '../../../Utility/infrastructure/showcase.model';

// Showcase: recapiti e modulo di contatto. Template: i testi sono in ShowcaseService.Contenuti.contact.
// Il modulo non passa dal server: prepara il messaggio nel programma di posta del visitatore.
@Component({
  selector: 'app-showcase-contact-page',
  templateUrl: './showcase-contact-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ShowcaseContactPageComponent {

  public readonly title: string;

  public nome = '';
  public email = '';
  public messaggio = '';

  constructor(public showcaseService: ShowcaseService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('showcase.menu.contact');
  }

  public get c(): ShowcaseContenuti['contact'] {
    return this.showcaseService.Contenuti.contact;
  }

  public get valido(): boolean {
    return this.nome.trim().length > 0 && /^\S+@\S+\.\S+$/.test(this.email.trim()) && this.messaggio.trim().length > 0;
  }

  public invia(): void {
    if (!this.valido)
      return;
    const oggetto = this.transloco.translate('showcase.contactPage.mailSubject', { name: this.nome.trim() });
    const testo = `${this.messaggio.trim()}\n\n${this.nome.trim()} - ${this.email.trim()}`;
    window.location.href = `mailto:${this.c.email}?subject=${encodeURIComponent(oggetto)}&body=${encodeURIComponent(testo)}`;
  }
}
