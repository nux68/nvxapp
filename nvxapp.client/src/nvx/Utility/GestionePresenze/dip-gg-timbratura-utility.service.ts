import { Injectable } from '@angular/core';
import { Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Body_Timbratura, Dip_GG_RichiestaModel, TipoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';

@Injectable({
  providedIn: 'root'
})
export class DipGGTimbraturaUtilityService {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) { }

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
            if (GG_Just.allDay)
              retText = retText;
            else
              retText = retText + " " + GG_Just.hhmm;

            return retText;

          }

          break;
        case TipoRichiesta.NotaSpesa:
          break;
      }


    }
    return '';


  }



}
