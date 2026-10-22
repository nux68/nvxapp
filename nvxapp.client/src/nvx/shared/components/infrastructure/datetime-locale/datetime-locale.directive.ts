import { Directive, ElementRef, OnInit, inject } from '@angular/core';
import { LanguageService } from '../../../../Utility/infrastructure/language.service';

/*
 Calendari (ion-datetime) nella lingua dell'interfaccia: nomi di mesi e giorni, formato dell'ora.
 Si applica da sola a ogni ion-datetime dei moduli che importano SharedComponentInfrastructureModule;
 un ion-datetime con un proprio attributo locale lo mantiene.
 Senza, Ionic userebbe la lingua del browser invece di quella scelta nell'applicazione.
*/
@Directive({
  // selettore sull'elemento di proposito: deve agganciarsi a ogni ion-datetime senza modificarne i template
  // eslint-disable-next-line @angular-eslint/directive-selector
  selector: 'ion-datetime:not([locale])',
  standalone: true,
})
export class NvxDatetimeLocaleDirective implements OnInit {

  private readonly element: ElementRef<HTMLIonDatetimeElement> = inject(ElementRef);
  private readonly languageService = inject(LanguageService);

  ngOnInit(): void {
    this.element.nativeElement.locale = this.languageService.locale;
  }
}
