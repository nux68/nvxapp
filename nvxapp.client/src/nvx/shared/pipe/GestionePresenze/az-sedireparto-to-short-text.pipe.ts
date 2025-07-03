import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Az_SediRepartoModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';

@Pipe({
  name: 'azSediRepartoToShortText',
  standalone: true
})
export class AzSediRepartoToShortTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) {}

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null);
    }
    const repartoId = value as number;
    const repartoList: Az_SediRepartoModel[] | null = this.sharedParameterService.Az_SediReparto;
    if (!repartoList || repartoList.length === 0) {
      return repartoId;
    }
    const found = repartoList.find(item => item.id === repartoId);
    return found ? found.descrizione : repartoId;
  }
}
