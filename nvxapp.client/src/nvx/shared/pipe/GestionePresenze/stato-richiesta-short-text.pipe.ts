import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

// translate() globale (non TranslocoService iniettato): la pipe viene creata anche con new fuori dalla DI;
// i testi sono calcolati una volta, il cambio lingua ricarica la pagina
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
        return translate('attendance.pipes.statoRichiestaShortText.diretta');
      case StatoRichiesta.Immessa:
        return translate('attendance.pipes.statoRichiestaShortText.immessa');
      case StatoRichiesta.Cancellata:
        return translate('attendance.pipes.statoRichiestaShortText.cancellata');
      case StatoRichiesta.Rifiutata:
        return translate('attendance.pipes.statoRichiestaShortText.rifiutata');
      case StatoRichiesta.ApprovazioneInCorso:
        return translate('attendance.pipes.statoRichiestaShortText.approvazioneInCorso');
      case StatoRichiesta.ParzialmenteApprovata:
        return translate('attendance.pipes.statoRichiestaShortText.parzialmenteApprovata');
      case StatoRichiesta.Approvata:
        return translate('attendance.pipes.statoRichiestaShortText.approvata');

      default:
        return '';
    }
  }

}
