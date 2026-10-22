import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import { TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';

// translate() globale (non TranslocoService iniettato): la pipe viene creata anche con new fuori dalla DI;
// i testi sono calcolati una volta, il cambio lingua ricarica la pagina
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
        return translate('attendance.pipes.tipoTimbraturaToLongText.entrata');
      case TipoTimbratura.Uscita:
        return translate('attendance.pipes.tipoTimbraturaToLongText.uscita');
      case TipoTimbratura.SenzaVerso:
        return translate('attendance.pipes.tipoTimbraturaToLongText.senzaVerso');
      case TipoTimbratura.Attivita:
        return translate('attendance.pipes.tipoTimbraturaToLongText.attivita');
      default:
        return '';
    }
  }

}
