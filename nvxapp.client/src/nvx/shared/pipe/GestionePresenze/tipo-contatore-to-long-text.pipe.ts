import { Pipe, PipeTransform } from '@angular/core';
import { TipoContatore } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';

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
      case TipoContatore.NoContatore:        return 'Nessun contatore';
      case TipoContatore.Contatore:          return 'Contatore';
      case TipoContatore.ContatoreConAvviso: return 'Contatore con avviso';
      case TipoContatore.ContatoreConBlocco: return 'Contatore con blocco';
      default:                               return '';
    }
  }

}
