import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { Par_CompetenzaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';

@Pipe({
  name: 'parCompetenzaToLongText',
  standalone: true
})
export class ParCompetenzaToLongTextPipe implements PipeTransform {
  constructor(private sharedParameterService: SharedParameterGestionePresenzeService) { }

  transform(value: unknown, ...args: unknown[]): string | number | null {
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null);
    }
    const competenzaId = value as number;
    const competenzaList: Par_CompetenzaModel[] | null = this.sharedParameterService.Par_Competenza;
    if (!competenzaList || competenzaList.length === 0) {
      return competenzaId;
    }
    const found = competenzaList.find(item => item.id === competenzaId);
    return found ? found.descrizione : competenzaId;
  }
}
