import { Pipe, PipeTransform } from '@angular/core';
import { TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';

@Pipe({
  name: 'tipoTimbraturaToShortText',
  standalone: true
})
export class TipoTimbraturaToShortTextPipe implements PipeTransform {

  transform(value: TipoTimbratura | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case TipoTimbratura.Entrata:
        return 'E';
      case TipoTimbratura.Uscita:
        return 'U';
      case TipoTimbratura.SenzaVerso:
        return 'S';
      case TipoTimbratura.Attivita:
        return 'A';
      default:
        return '';
    }
  }

}
