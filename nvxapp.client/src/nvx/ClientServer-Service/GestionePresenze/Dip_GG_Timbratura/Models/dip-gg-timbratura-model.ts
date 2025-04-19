import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_TimbraturaInModel {
  
}

export class Dip_GG_TimbraturaOutModel extends ModelResult {

  public Dip_GG_TimbraturaModel: Dip_GG_TimbraturaModel;

}

export class Dip_GG_TimbraturaModel  {

  
  // Proprietà richieste
  public idDip_RapportoLavoro!: number; // Utilizzo di "!" per proprietà richieste non nullable

  // Date e timbrature
  public timbratura!: Date;
  public timbraturaOriginale!: Date;
  public timbraturaArrotondata?: Date; // Nullable
  public giornoCompetenza!: Date; // Giorno per cavallo notte montanti/smontanti

  // Tipo di timbratura
  public timbraturaTipo!: TipoTimbratura;

  /*
    Per gli inserimenti diretti:
      richiestaStato = Diretta
      idDip_Richiesta = null
  */
  public richiestaStato!: StatoRichiesta;
  public idDip_Richiesta?: number; // Nullable



}

export interface iDip_GG_TimbraturaModel {
  // Proprietà richieste
  idDip_RapportoLavoro: number; // Non nullable

  // Date e timbrature
  timbratura: Date;
  timbraturaOriginale: Date;
  timbraturaArrotondata?: Date; // Nullable
  giornoCompetenza: Date; // Giorno per cavallo notte montanti/smontanti

  // Tipo di timbratura
  timbraturaTipo: TipoTimbratura;

  /*
    Per gli inserimenti diretti:
      richiestaStato = Diretta
      idDip_Richiesta = null
  */
  richiestaStato: StatoRichiesta;
  idDip_Richiesta?: number; // Nullable
}

export enum TipoTimbratura {
  Entrata,     
  Uscita,      
  SenzaVerso,  
  Attivita     
}
