import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_CausaliModel {
    public id!: number;
    public idAz_Anagrafica!: number;
    public descrizione!: string;
    public codice!: string;
}

export class Par_CausaliInModel {
}

export class Par_CausaliOutModel extends ModelResult {
    public par_Causali: Par_CausaliModel[];
}

export class Par_CausaliGetInModel {
    public id: number;
}

export class Par_CausaliGetOutModel extends ModelResult {
    public par_Causale: Par_CausaliModel;
}

export class Par_CausaliPutInModel {
    public par_Causale: Par_CausaliModel;
}

export class Par_CausaliPutOutModel extends ModelResult {
    public par_Causale: Par_CausaliModel;
}

export class Par_Causali_DeleteInModel {
  id: number;
}
export class Par_Causali_DeleteOutModel extends ModelResult {
}
