import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import { TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

// translate() globale (non TranslocoService iniettato): la pipe viene creata anche con new fuori dalla DI;
// i testi sono calcolati una volta, il cambio lingua ricarica la pagina
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
        return translate('attendance.pipes.tipoRichiestaToLongText.timbratura');
      case TipoRichiesta.Giustificativo:
        return translate('attendance.pipes.tipoRichiestaToLongText.giustificativo');
      case TipoRichiesta.NotaSpesa:
        return translate('attendance.pipes.tipoRichiestaToLongText.notaSpesa');

      default:
        return '';
    }
  }

}
