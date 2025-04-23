import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";



export class Dip_GG_GiustificativiInModel {
  
}

export class Dip_GG_GiustificativiOutModel extends ModelResult {

  public Dip_GG_GiustificativiModel: Dip_GG_GiustificativiModel;

}

export class Dip_GG_GiustificativiModel  {

  public id: number; // Required
  public idDip_RapportoLavoro: number; // Required
  public data: Date;
  public idJustificationType: number;
  public inputType: JustificationInputType;

  public hours?: string;
  public from?: string;

  public idPar_Giustificativi: number; // Required
  public richiestaStato: StatoRichiesta;
  public idDip_Richiesta?: number; // Opzionale (può essere null)

}

export enum JustificationInputType {
  Manual,
  AllDay,
  IntegrateDay
}
