import { Pipe, PipeTransform } from '@angular/core';
import { Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';

@Pipe({
  name: 'parCausaliToLongText',
  standalone: true
})
export class ParCausaliToLongTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    // Validate input type (basic check)
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null); // Return original value or null if not a valid number
    }

    const causaliId = value as number;

    // Get the list of causali from the shared service
    const causaliList: Par_CausaliModel[] | null = this.sharedParameterService.Par_Causali;

    // Check if the list is available
    if (!causaliList || causaliList.length === 0) {
      return causaliId;
    }

    // Find the item in the list matching the ID
    const foundCausale = causaliList.find(item => item.id === causaliId);

    // Return the description if found, otherwise return the original ID as a fallback
    if (foundCausale) {
      return foundCausale.descrizione;
    } else {
      return causaliId;
    }
  }

}
