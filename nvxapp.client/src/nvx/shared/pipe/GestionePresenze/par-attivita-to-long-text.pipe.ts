import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';

@Pipe({
  name: 'parAttivitaToLongText',
  standalone: true
})
export class ParAttivitaToLongTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null);
    }
    const attivitaId = value as number;
    const attivitaList: Par_AttivitaModel[] | null = this.sharedParameterService.Par_Attivita;
    if (!attivitaList || attivitaList.length === 0) {
      return attivitaId;
    }
    const found = attivitaList.find(item => item.id === attivitaId);
    return found ? found.descrizione : attivitaId;
  }
}
