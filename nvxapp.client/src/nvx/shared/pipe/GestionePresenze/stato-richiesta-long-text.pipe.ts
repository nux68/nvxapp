import { Pipe, PipeTransform } from '@angular/core';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

@Pipe({
  name: 'statoRichiestaLongText',
  standalone: true
})

export class StatoRichiestaLongTextPipe implements PipeTransform {

  transform(value: StatoRichiesta | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }

    switch (value) {
      case StatoRichiesta.Diretta:
        return 'Diretta';
      case StatoRichiesta.Immessa:
        return 'Immessa';
      case StatoRichiesta.Cancellata:
        return 'Cancellata';
      case StatoRichiesta.Rifiutata:
        return 'Rifiutata';
      case StatoRichiesta.ApprovazioneInCorso:
        return 'Approvazione In Corso';
      case StatoRichiesta.ParzialmenteApprovata:
        return 'Parzialmente Approvata';
      case StatoRichiesta.Approvata:
        return 'Approvata';

      default:
        return '';
    }
  }

}

function Injectable(arg0: { providedIn: string; }): (target: typeof StatoRichiestaLongTextPipe) => void | typeof StatoRichiestaLongTextPipe {
    throw new Error('Function not implemented.');
}
