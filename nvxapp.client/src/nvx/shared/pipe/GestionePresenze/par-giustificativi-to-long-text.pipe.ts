import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';

@Pipe({
  name: 'parGiustificativiToLongText',
  standalone: true
})
export class ParGiustificativiToLongTextPipe implements PipeTransform {

  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    // Validate input type (basic check)
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null); // Return original value or null if not a valid number
    }

    const giustificativiId = value as number;

    // Get the list of giustificativi from the shared service
    const giustificativiList: Par_GiustificativiModel[] | null = this.sharedParameterService.Par_Giustificativi;

    // Check if the list is available
    if (!giustificativiList || giustificativiList.length === 0) {
      return giustificativiId;
    }

    // Find the item in the list matching the ID
    const foundGiustificativo = giustificativiList.find(item => item.id === giustificativiId);

    // Return the description if found, otherwise return the original ID as a fallback
    if (foundGiustificativo) {
      return foundGiustificativo.descrizione;
    } else {
      return giustificativiId; 
    }
  }

}
