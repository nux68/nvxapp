import { ModelResult } from "../../../ModelsBase/model-result";

export class Dip_RapportoLavoroModel {
  public id!: number;
  public idDip_Anagrafica!: number;
  public idAz_SubCommessaAttivita!: number;
  public dataAss: string | null = null; // assunzione, "YYYY-MM-DDT00:00:00"
  public dataLic: string | null = null; // licenziamento

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



