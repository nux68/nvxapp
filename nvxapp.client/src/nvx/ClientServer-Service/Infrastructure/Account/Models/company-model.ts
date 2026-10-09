import { ApplicationType } from './user-load-model';
import { ModelResult } from "../../../ModelsBase/model-result";
import { RolesModel } from "./user-roles-model";


export class CompanyListModel {
  constructor(
    public idCompany: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false
  ) { }
}
export class CompanyListInModel {
  
}
export class CompanyListOutModel extends ModelResult {

  public companyList: CompanyListModel[] ;

}



export class CompanyEditModel {
  constructor(
    public idCompany: number = 0,
    public descrizione: string | null = "",
    public idAspNetUsers: string = "",
    public mainUser: boolean = false,
    public mail: string | null = null,
    public pw: string | null = null,
    // tutti gli applicativi (dall'enum del server) con lo stato per l'azienda
    public applications: CompanyApplicationModel[] = []
  ) { }
}

// applicativo con lo stato per l'azienda (CompanyApplicationModel del server)
export class CompanyApplicationModel {
  public idCompany: number = 0;
  public applicationType: ApplicationType = ApplicationType.Moke;
  public descrizione: string = '';
  public active: boolean = false;
  public activationDate: string | null = null;
  public deactivationDate: string | null = null;
}
export class CompanyGetInModel {
  public id: number;
}
export class CompanyGetOutModel extends ModelResult {

  public companyEdit: CompanyEditModel;

}
export class CompanyPutInModel  {

  public companyEdit: CompanyEditModel;

}
export class CompanyPutOutModel extends ModelResult {

  public companyEdit: CompanyEditModel;

}




