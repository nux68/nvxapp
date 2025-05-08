import { Pipe, PipeTransform } from '@angular/core';
import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';

@Pipe({
  name: 'idAspNetUsersToNome',
  standalone: true
})
export class IdAspNetUsersToNomePipe implements PipeTransform {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,) {

  }

  transform(value: unknown, ...args: unknown[]): unknown {


    // Validate input type (basic check)
    if (typeof value !== 'string' || value == null) {
      return value as (string | null); // Return original value or null if not a valid number
    }

    const record = this.sharedParameterGestionePresenzeService.Dip_Anagrafica?.find(dip =>
      dip.idAspNetUsers === value
    );

    if (record != null) {
      return record.nome ?? record.userName;
    }

    return null;
  }

}
