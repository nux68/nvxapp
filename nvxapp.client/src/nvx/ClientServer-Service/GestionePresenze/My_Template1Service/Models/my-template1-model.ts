import { ModelResult } from "../../../ModelsBase/model-result";

export class My_Template1Model {

  public id: number;
  public idAz_Anagrafica!: number;
  public descrizione!: string; 

}

export class My_template1_GetAllInModel {
  
}

export class My_template1_GetAllOutModel extends ModelResult {

  public my_Template1: My_Template1Model[];

}


export class My_template1_GetInModel {
  public id: number;
}
export class My_template1_GetOutModel extends ModelResult {
  public my_Template1: My_Template1Model;
}
export class My_template1_PutInModel {
  public my_Template1: My_Template1Model;
}
export class My_template1_PutOutModel extends ModelResult {
  public my_Template1: My_Template1Model;
}

// DELETE MODELS
export class My_template1_DeleteInModel {
  public id: number;
}
export class My_template1_DeleteOutModel extends ModelResult {
  public my_Template1: My_Template1Model;
}


