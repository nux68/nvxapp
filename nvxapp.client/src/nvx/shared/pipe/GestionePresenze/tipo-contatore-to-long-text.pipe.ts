import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import { TipoContatore } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';

// translate() globale (non TranslocoService iniettato): la pipe viene creata anche con new fuori dalla DI;
// i testi sono calcolati una volta, il cambio lingua ricarica la pagina
@Pipe({
  name: 'tipoContatoreToLongText',
  standalone: true
})
export class TipoContatoreToLongTextPipe implements PipeTransform {

  transform(value: TipoContatore | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case TipoContatore.NoContatore:        return translate('attendance.pipes.tipoContatoreToLongText.noContatore');
      case TipoContatore.Contatore:          return translate('attendance.pipes.tipoContatoreToLongText.contatore');
      case TipoContatore.ContatoreConAvviso: return translate('attendance.pipes.tipoContatoreToLongText.contatoreConAvviso');
      case TipoContatore.ContatoreConBlocco: return translate('attendance.pipes.tipoContatoreToLongText.contatoreConBlocco');
      default:                               return '';
    }
  }

}
