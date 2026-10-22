import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

// translate() globale (non TranslocoService iniettato): la pipe viene creata anche con new fuori dalla DI;
// i testi sono calcolati una volta, il cambio lingua ricarica la pagina
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
        return translate('attendance.pipes.statoRichiestaLongText.diretta');
      case StatoRichiesta.Immessa:
        return translate('attendance.pipes.statoRichiestaLongText.immessa');
      case StatoRichiesta.Cancellata:
        return translate('attendance.pipes.statoRichiestaLongText.cancellata');
      case StatoRichiesta.Rifiutata:
        return translate('attendance.pipes.statoRichiestaLongText.rifiutata');
      case StatoRichiesta.ApprovazioneInCorso:
        return translate('attendance.pipes.statoRichiestaLongText.approvazioneInCorso');
      case StatoRichiesta.ParzialmenteApprovata:
        return translate('attendance.pipes.statoRichiestaLongText.parzialmenteApprovata');
      case StatoRichiesta.Approvata:
        return translate('attendance.pipes.statoRichiestaLongText.approvata');

      default:
        return '';
    }
  }

}

function Injectable(arg0: { providedIn: string; }): (target: typeof StatoRichiestaLongTextPipe) => void | typeof StatoRichiestaLongTextPipe {
    throw new Error('Function not implemented.');
}
