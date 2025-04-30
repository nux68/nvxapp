import { Pipe, PipeTransform } from '@angular/core';
import { TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

@Pipe({
  name: 'tipoRichiestaToLongText',
  standalone: true
})
export class TipoRichiestaToLongTextPipe implements PipeTransform {

  transform(value: TipoRichiesta | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case TipoRichiesta.Timbratura:
        return 'Timbratura';
      case TipoRichiesta.Giustificativo:
        return 'Giustificativo';
      case TipoRichiesta.NotaSpesa:
        return 'NotaSpesa';

      default:
        return '';
    }
  }

}
