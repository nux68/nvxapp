import { ModelResult } from "../../../ModelsBase/model-result";


export class CompanyMoveDealerModel {
  public idDealer: number = 0;
  public descrizione: string | null = '';
}

export class CompanyMoveFinancialAdvisorModel {
  public idFinancialAdvisor: number = 0;
  public idDealer: number = 0;
  public descrizione: string | null = '';
}

export class CompanyMoveCompanyModel {
  public idCompany: number = 0;
  public idFinancialAdvisor: number = 0;
  public descrizione: string | null = '';
}


// tutta la gerarchia centri -> studi -> aziende (riservato al SuperUser)
export class CompanyMoveTreeInModel {
}

export class CompanyMoveTreeOutModel extends ModelResult {
  public dealers: CompanyMoveDealerModel[] = [];
  public financialAdvisors: CompanyMoveFinancialAdvisorModel[] = [];
  public companies: CompanyMoveCompanyModel[] = [];
}


// sposta l'azienda sotto un altro studio
export class CompanyMovePutInModel {
  public idCompany: number = 0;
  public idFinancialAdvisor: number = 0;
}

export class CompanyMovePutOutModel extends ModelResult {
  public company: CompanyMoveCompanyModel | null = null;
}
