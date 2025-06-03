import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Az_CommessaModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';

@Pipe({
  name: 'azCommessaToLongText',
  standalone: true
})
export class AzCommessaToLongTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null);
    }
    const commessaId = value as number;
    const commessaList: Az_CommessaModel[] | null = this.sharedParameterService.Az_Commessa;
    if (!commessaList || commessaList.length === 0) {
      return commessaId;
    }
    const found = commessaList.find(item => item.id === commessaId);
    return found ? found.descrizione : commessaId;
  }
}
