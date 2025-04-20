import { Pipe, PipeTransform } from '@angular/core';
import { TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';

@Pipe({
  name: 'tipoTimbraturaToLongText',
  standalone: true
})
export class TipoTimbraturaToLongTextPipe implements PipeTransform {

  transform(value: TipoTimbratura | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case TipoTimbratura.Entrata:
        return 'Entrata';
      case TipoTimbratura.Uscita:
        return 'Uscita';
      case TipoTimbratura.SenzaVerso:
        return 'SenzaVerso';
      case TipoTimbratura.Attivita:
        return 'Attivita';
      default:
        return '';
    }
  }

}
