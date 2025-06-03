import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';

@Pipe({
  name: 'azClienteToLongText',
  standalone: true
})
export class AzClienteToLongTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null);
    }
    const clienteId = value as number;
    const clienteList: Az_ClienteModel[] | null = this.sharedParameterService.Az_Cliente;
    if (!clienteList || clienteList.length === 0) {
      return clienteId;
    }
    const found = clienteList.find(item => item.id === clienteId);
    return found ? found.descrizione : clienteId;
  }
}
