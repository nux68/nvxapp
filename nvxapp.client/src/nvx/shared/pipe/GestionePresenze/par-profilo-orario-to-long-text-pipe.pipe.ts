import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Par_ProfiloOrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';

@Pipe({
  name: 'parProfiloOrarioToLongTextPipe',
  standalone: true
})
export class ParProfiloOrarioToLongTextPipePipe implements PipeTransform {

  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    // Validate input type (basic check)
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null); // Return original value or null if not a valid number
    }

    const par_ProfiloOrarioId = value as number;

    // Get the list of giustificativi from the shared service
    const par_ProfiloOrarioList: Par_ProfiloOrarioModel[] | null = this.sharedParameterService.Par_ProfiloOrario;

    // Check if the list is available
    if (!par_ProfiloOrarioList || par_ProfiloOrarioList.length === 0) {
      return par_ProfiloOrarioId;
    }

    // Find the item in the list matching the ID
    const foundPar_ProfiloOrario = par_ProfiloOrarioList.find(item => item.id === par_ProfiloOrarioId);

    // Return the description if found, otherwise return the original ID as a fallback
    if (foundPar_ProfiloOrario) {
      return foundPar_ProfiloOrario.descrizione;
    } else {
      return par_ProfiloOrarioId;
    }
  }

}
