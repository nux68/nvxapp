import { CheckObjOn_Id_Number } from "../../../ModelsBase/check-obj";
import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_AttivitaModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  descrizione: string = '';
  public backgroundColor!: string;
  public textColor!: string;
  public default: boolean;
}

export class Par_Attivita_GetAll_InModel {}

export class Par_Attivita_GetAll_OutModel extends ModelResult
{
  public par_Attivita: Par_AttivitaModel[] = [];
}

export class Par_AttivitaGetInModel
{
    id: number = 0;
}
//export class Par_AttivitaGetOutModel extends ModelResult
//{
//  public par_Attivita: Par_AttivitaModel = new Par_AttivitaModel();
//  public par_Competenza: CheckObjOn_Id_Number[];
//}
export class Par_AttivitaGetOutModel extends ModelResult {
  public par_Attivita: Par_AttivitaModel;
  public par_Competenza: CheckObjOn_Id_Number[];

  constructor() {
    super();
    this.par_Attivita = new Par_AttivitaModel();
    this.par_Competenza = [];
  }
}
export class Par_AttivitaPutInModel
{
  public par_Attivita: Par_AttivitaModel = new Par_AttivitaModel();
  public par_Competenza: CheckObjOn_Id_Number[];
}
export class Par_AttivitaPutOutModel extends ModelResult
{
  public par_Attivita: Par_AttivitaModel = new Par_AttivitaModel();
  public par_Competenza: CheckObjOn_Id_Number[];
}

export class Par_AttivitaDeleteInModel
{
  public id: number = 0;
}

export class Par_AttivitaDeleteOutModel extends ModelResult
{
  public par_Attivita: Par_AttivitaModel = new Par_AttivitaModel();
}

export class Par_Attivita_Get_4User_InModel {
  public giorno: Date = new Date();
  public userId: string = '';
}

export class Par_Attivita_Get_4User_OutModel extends ModelResult {
  public par_Attivita: Par_AttivitaModel[] = [];
}
