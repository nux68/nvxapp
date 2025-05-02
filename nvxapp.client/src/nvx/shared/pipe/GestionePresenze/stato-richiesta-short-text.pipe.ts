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
        return 'Dir.';
      case StatoRichiesta.Immessa:
        return 'Imm.';
      case StatoRichiesta.Cancellata:
        return 'Canc.';
      case StatoRichiesta.Rifiutata:
        return 'Rif.';
      case StatoRichiesta.ApprovazioneInCorso:
        return 'In Appr.';
      case StatoRichiesta.ParzialmenteApprovata:
        return 'Parz. Appr';
      case StatoRichiesta.Approvata:
        return 'Appr.';

      default:
        return '';
    }
  }

}
