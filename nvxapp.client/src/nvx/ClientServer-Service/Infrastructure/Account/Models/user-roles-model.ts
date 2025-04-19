



export class UserRolesInModel {

}

export class UserRolesOutModel {
  
  public roles: RolesModel[];

}

export class RolesModel {
  public name: string;
  public code: RoleCode;
}

export enum RoleCode {

  User = 0,

  CompanyAdmin = 10,
  CompanyPowerAdmin = 11,

  FinancialAdvisorAdmin = 100,
  FinancialAdvisorPowerAdmin = 101,

  DealerAdmin = 1000,
  DealerPowerAdmin = 1001,

  Admin = 10000,
  PowerAdmin = 10001,

  SuperUser = 100000

}
