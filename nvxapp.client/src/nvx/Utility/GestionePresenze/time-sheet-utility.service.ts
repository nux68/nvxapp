import { Injectable } from '@angular/core';
import { LanguageService } from '../infrastructure/language.service';
import { Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Body_Timbratura, Dip_GG_RichiestaModel, riepilogoNotaSpesa, TipoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';

@Injectable({
  providedIn: 'root'
})
export class TimeSheetUtilityService {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private languageService: LanguageService) { }

  public getDatiObj(item: Dip_GG_RichiestaModel): any {

    if (item.dati) 
        return JSON.parse(item.dati);
    

    return null;

  }

  public getDatitext(item: Dip_GG_RichiestaModel): string {

    if (item.dati) {
      const dati = this.getDatiObj(item);
      switch (item.richiestaTipo) {
        case TipoRichiesta.Timbratura:
          const Timbratura = dati as Dip_GG_Richiesta_Body_Timbratura;
          return Timbratura.hhmm;

          break;
        case TipoRichiesta.Giustificativo:
          const GG_Just = dati as Dip_GG_Richiesta_Body_Giustificativo;

          const Just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == GG_Just.idPar_Giustificativi);
          if (Just) {
            let  retText = Just.codice;
            // a importo: l'importo per giorno al posto delle ore
            if (GG_Just.importo != null)
              retText = retText + " " + new Intl.NumberFormat(this.languageService.locale, { style: 'currency', currency: 'EUR' }).format(GG_Just.importo);
            else if (GG_Just.allDay)
              retText = retText;
            else
              retText = retText + " " + GG_Just.hhmm;

            return retText;

          }

          break;
        case TipoRichiesta.NotaSpesa: {
          const nota = riepilogoNotaSpesa(item.dati);
          const totale = nota.totale.toLocaleString(this.languageService.locale, { minimumFractionDigits: 2, maximumFractionDigits: 2, useGrouping: false });
          return `${nota.descrizione} € ${totale}`.trim();
        }
      }


    }
    return '';


  }



}
