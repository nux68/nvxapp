import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';

@Pipe({
  name: 'idDipRapportoLavoroToNome',
  standalone: true
})
export class IdDipRapportoLavoroToNomePipe implements PipeTransform {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,) {

  }

  transform(value: unknown, ...args: unknown[]): unknown {


    // Validate input type (basic check)
    if (typeof value !== 'number' || isNaN(value)) {
      return value as (number | null); // Return original value or null if not a valid number
    }

    const record = this.sharedParameterGestionePresenzeService.Dip_Anagrafica?.find(dip =>
      dip.dip_RapportoLavoro.some(rapporto => rapporto.id === value)
    );

    if (record != null) {
      return record.nome??record.userName;
    }

    return null;
  }

}
