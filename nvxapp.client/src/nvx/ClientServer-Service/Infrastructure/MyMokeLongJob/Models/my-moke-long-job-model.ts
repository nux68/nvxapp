import { ModelResult } from "../../../ModelsBase/model-result";



export class MyMokeLongJobModel {
  constructor(
    public jobParameter: string = ""
  ) { }
}

export class MyMokeLongJobInModel {
  
}
export class MyMokeLongJobOutModel extends ModelResult {

  public jobId: string = ""

}



