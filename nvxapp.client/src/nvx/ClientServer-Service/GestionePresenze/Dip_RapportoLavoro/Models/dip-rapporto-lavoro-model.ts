import { ModelResult } from "../../../ModelsBase/model-result";

export class Dip_RapportoLavoroModel {
  public id!: number;
  public idDip_Anagrafica!: number;
  public idAz_SubCommessaAttivita!: number;

}

export class Dip_RapportoLavoro_Get_InModel {
  id: number; // = IdDip_Anagrafica
}
export class Dip_RapportoLavoro_Get_OutModel extends ModelResult {
  dip_RapportoLavoro: Dip_RapportoLavoroModel[] = [];
}

export class Dip_RapportoLavoro_Put_InModel {
  id: number; // = IdDip_Anagrafica
  dip_RapportoLavoro: Dip_RapportoLavoroModel[] = [];
}
export class Dip_RapportoLavoro_Put_OutModel extends ModelResult {
  id: number; // = IdDip_Anagrafica
  dip_RapportoLavoro: Dip_RapportoLavoroModel[] = [];
}



