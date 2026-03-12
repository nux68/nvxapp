import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_TimbraturaModel {

  public id: number;
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
      idDip_GG_Richiesta = null
  */
  public richiestaStato!: StatoRichiesta;
  public idDip_GG_Richiesta?: number; // Nullable



}
export enum TipoTimbratura {
  Entrata,
  Uscita,
  SenzaVerso,
  Attivita
}



export class Dip_GG_Timbratura_GetAll_InModel {
  public idAspNetUsers?: string
  public year: number;
  public month: number;
}
export class Dip_GG_Timbratura_GetAll_OutModel extends ModelResult {

  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[];

}


export class Dip_GG_Timbratura_Stamp_InModel {
  public dateStamp!: string;
}
export class Dip_GG_Timbratura_Stamp_OutModel extends ModelResult {

  
}













