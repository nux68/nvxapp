import { ModelResult } from "../../../ModelsBase/model-result";
import { MenuRestrictionModel } from "../../Account/Models/user-load-model";


// esclusioni dell'azienda corrente (riservato al CompanyPowerAdmin)
export class CompanyMenuRestrictionListInModel {
}

export class CompanyMenuRestrictionListOutModel extends ModelResult {
  public companyMenuRestriction: MenuRestrictionModel[] = [];
}


// sostituisce tutte le esclusioni dell'azienda corrente
export class CompanyMenuRestrictionPutInModel {
  public companyMenuRestriction: MenuRestrictionModel[] = [];
}

export class CompanyMenuRestrictionPutOutModel extends ModelResult {
  public companyMenuRestriction: MenuRestrictionModel[] = [];
}
