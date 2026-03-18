import { Pipe, PipeTransform } from '@angular/core';
import { Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';

@Pipe({
  name: 'parCausaliToShortText',
  standalone: true
})
export class ParCausaliToShortTextPipe implements PipeTransform {

  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: number | null | undefined): string|number {
    if (value === null || value === undefined) {
      return '';
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
      return foundCausale.codice;
    } else {
      return causaliId;
    }
  }

}
