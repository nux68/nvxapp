import { Pipe, PipeTransform } from '@angular/core';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

@Pipe({
  name: 'statoRichiestaShortText',
  standalone: true
})
export class StatoRichiestaShortTextPipe implements PipeTransform {

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
        return 'ApprovazioneInCorso';
      case StatoRichiesta.ParzialmenteApprovata:
        return 'ParzialmenteApprovata';
      case StatoRichiesta.Approvata:
        return 'Approvata';

      default:
        return '';
    }
  }

}
