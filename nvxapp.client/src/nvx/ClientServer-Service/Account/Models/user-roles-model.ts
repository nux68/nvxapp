



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

  DealerAdmin = 100,
  DealerPowerAdmin = 101,

  Admin = 1000,
  PowerAdmin = 1001,

  SuperUser = 10000

}
