import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';

@Pipe({
  name: 'parOrarioToLongText',
  standalone: true
})
export class ParOrarioToLongTextPipe implements PipeTransform {

  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null); // Return original value or null if not a valid number
    }

    const par_OrarioId = value as number;

    // Get the list of giustificativi from the shared service
    const par_OrarioList: Par_OrarioModel[] | null = this.sharedParameterService.Par_Orario;

    // Check if the list is available
    if (!par_OrarioList || par_OrarioList.length === 0) {
      return par_OrarioId;
    }

    // Find the item in the list matching the ID
    const foundPar_Orario = par_OrarioList.find(item => item.id === par_OrarioId);

    // Return the description if found, otherwise return the original ID as a fallback
    if (foundPar_Orario) {
      return foundPar_Orario.descrizione;
    } else {
      return par_OrarioId;
    }


  }

}
