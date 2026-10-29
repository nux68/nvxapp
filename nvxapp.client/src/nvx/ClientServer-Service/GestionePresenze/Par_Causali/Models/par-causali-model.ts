import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_CausaliModel {
    public id!: number;
    public idAz_Anagrafica!: number;
    public descrizione!: string;
    public codice!: string;
    /** ore oppure importo in euro (non si cambia se la causale e' gia' usata) */
    public tipo: TipoValore = TipoValore.Orario;
}

/** valore di causali e giustificativi: ore oppure un importo in euro (2 decimali) */
export enum TipoValore {
    Orario = 0,
    Importo = 1
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
