import { ModelResult } from "../../../ModelsBase/model-result";



export class Par_OrarioIntervalloHHModel {

  id: number;
  idPar_Orario: number;

  public dalle: string | null;
  public alle: string | null;

  public dalle_Limite_SX: string | null;
  public dalle_Limite_DX: string | null;

  public alle_Limite_SX: string | null;
  public alle_Limite_DX: string | null;

  numCoppia: number;
}


export class Par_OrarioIntervalloHHInModel {
  
}
export class Par_OrarioIntervalloHHOutModel extends ModelResult {

  public par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] = [];

}



// Modelli per la gestione dell'edit
export class Par_OrarioIntervalloHH_Get_4Edit_InModel {
  id: number = 0; // id del profilo
}
export class Par_OrarioIntervalloHH_Get_4Edit_OutModel extends ModelResult {
  par_OrarioIntervalloHHM: Par_OrarioIntervalloHHModel[] = [];
}

export class Par_OrarioIntervalloHH_Put_4Edit_InModel {
  id: number = 0; // id del profilo
  par_OrarioIntervalloHHM: Par_OrarioIntervalloHHModel[] = [];
}
export class Par_OrarioIntervalloHH_Put_4Edit_OutModel extends ModelResult {
  id: number = 0; // id del profilo
  par_OrarioIntervalloHHM: Par_OrarioIntervalloHHModel[] = [];
}


export class Par_OrarioIntervalloHH_Arrange_Coppie_InModel {
  id: number = 0; // id del profilo
  numCoppie: number = 0; 
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] = [];
}
export class Par_OrarioIntervalloHH_Arrange_Coppie_OutModel extends ModelResult {
  id: number = 0; // id del profilo
  par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] = [];
}
