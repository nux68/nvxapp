import { ModelResult } from "../../../ModelsBase/model-result";

export class My_Template1Model {

  public id: number;
  public idDip_RapportoLavoro!: number; 

}

export class My_template1InModel {
  
}

export class My_template1OutModel extends ModelResult {

  public my_Template1Model: My_Template1Model;

}



