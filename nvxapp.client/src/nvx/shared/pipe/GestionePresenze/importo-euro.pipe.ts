import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from '../../../Utility/infrastructure/language.service';

/** importo di giustificativi e causali a importo: "€ 25,50" (formato della lingua attiva); vuoto se manca */
@Pipe({
  name: 'importoEuro',
  standalone: true
})
export class ImportoEuroPipe implements PipeTransform {

  constructor(private languageService: LanguageService) { }

  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }
    return new Intl.NumberFormat(this.languageService.locale, { style: 'currency', currency: 'EUR' }).format(value);
  }

}
