import { ModelResult } from "../../../ModelsBase/model-result";



export class Dip_GG_RichiestaInModel {
  
}

export class Dip_GG_RichiestaOutModel extends ModelResult {

  public Dip_GG_RichiestaModel: Dip_GG_RichiestaModel;

}

export class Dip_GG_RichiestaModel  {
  

}

export enum TipoRichiesta {
  Timbratura,
  Giustificativo,
  NotaSpesa
}

export enum StatoRichiesta {
  Diretta,

  Immessa,
  Cancellata,
  Rifiutata,
  ApprovazioneInCorso,
  ParzialmenteApprovata,

  Approvata
}
