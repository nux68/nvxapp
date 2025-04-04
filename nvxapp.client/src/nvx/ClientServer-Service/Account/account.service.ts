import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { UserRolesInModel, UserRolesOutModel } from './Models/user-roles-model';
import { LoginInModel, LoginOutModel } from './Models/login-model';
import { AuthService } from '../../Utility/auth.service';
import { UserLoadInModel, UserLoadOutModel } from './Models/user-load-model';
import { DealerGetInModel, DealerGetOutModel, DealerListInModel, DealerListOutModel, DealerPutInModel, DealerPutOutModel } from './Models/dealer-model';
import { CompanyGetInModel, CompanyGetOutModel, CompanyListInModel, CompanyListOutModel, CompanyPutInModel, CompanyPutOutModel } from './Models/company-model';
import { UserCompanyGetInModel, UserCompanyGetOutModel, UserCompanyListInModel, UserCompanyListOutModel, UserCompanyPutInModel, UserCompanyPutOutModel } from './Models/user-company-model';
import { FinancialAdvisorGetInModel, FinancialAdvisorGetOutModel, FinancialAdvisorListInModel, FinancialAdvisorListOutModel, FinancialAdvisorPutInModel, FinancialAdvisorPutOutModel } from './Models/financial-advisor-model';


@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor(private http: HttpClient,
              private authService: AuthService
  ) { }

  Login(model: GenericRequest<LoginInModel>): Observable<GenericResult<LoginOutModel>> {

    return this.http.post<GenericResult<LoginOutModel>>(environment.remoteData.apiUri + 'Account/Login', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  UserRoles(model: GenericRequest<UserRolesInModel>): Observable<GenericResult<UserRolesOutModel>> {

    return this.http.post<GenericResult<UserRolesOutModel>>(environment.remoteData.apiUri + 'Account/UserRoles', model)
      .pipe(
                map(r => {
                          // this.authService.setRole(r.data.roles);
                          return r;
                         }
                )
            );

  }

  UserLoad(model: GenericRequest<UserLoadInModel>): Observable<GenericResult<UserLoadOutModel>> {

    return this.http.post<GenericResult<UserLoadOutModel>>(environment.remoteData.apiUri + 'Account/UserLoad', model)
      .pipe(
                map(r => {
                            this.authService.Token = r.data.token;
                            return r;
                          }
                )
            );

  }


  DealerList(model: GenericRequest<DealerListInModel>): Observable<GenericResult<DealerListOutModel>> {

    return this.http.post<GenericResult<DealerListOutModel>>(environment.remoteData.apiUri + 'Account/DealerList', model)
      .pipe(
        map(r => {
          //this.authService.setRole(r.data.roles);
          return r;
        }
        )
      );

  }

  DealerGet(model: GenericRequest<DealerGetInModel>): Observable<GenericResult<DealerGetOutModel>> {

    return this.http.post<GenericResult<DealerGetOutModel>>(environment.remoteData.apiUri + 'Account/DealerGet', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  DealerPut(model: GenericRequest<DealerPutInModel>): Observable<GenericResult<DealerPutOutModel>> {

    return this.http.post<GenericResult<DealerPutOutModel>>(environment.remoteData.apiUri + 'Account/DealerPut', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }



  FinancialAdvisorList(model: GenericRequest<FinancialAdvisorListInModel>): Observable<GenericResult<FinancialAdvisorListOutModel>> {

    return this.http.post<GenericResult<FinancialAdvisorListOutModel>>(environment.remoteData.apiUri + 'Account/FinancialAdvisorList', model)
      .pipe(
        map(r => {
          //this.authService.setRole(r.data.roles);
          return r;
        }
        )
      );

  }

  FinancialAdvisorGet(model: GenericRequest<FinancialAdvisorGetInModel>): Observable<GenericResult<FinancialAdvisorGetOutModel>> {

    return this.http.post<GenericResult<FinancialAdvisorGetOutModel>>(environment.remoteData.apiUri + 'Account/FinancialAdvisorGet', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  FinancialAdvisorPut(model: GenericRequest<FinancialAdvisorPutInModel>): Observable<GenericResult<FinancialAdvisorPutOutModel>> {

    return this.http.post<GenericResult<FinancialAdvisorPutOutModel>>(environment.remoteData.apiUri + 'Account/FinancialAdvisorPut', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }




  CompanyList(model: GenericRequest<CompanyListInModel>): Observable<GenericResult<CompanyListOutModel>> {

    return this.http.post<GenericResult<CompanyListOutModel>>(environment.remoteData.apiUri + 'Account/CompanyList', model)
      .pipe(
        map(r => {
          //this.authService.setRole(r.data.roles);
          return r;
        }
        )
      );

  }

  CompanyGet(model: GenericRequest<CompanyGetInModel>): Observable<GenericResult<CompanyGetOutModel>> {

    return this.http.post<GenericResult<CompanyGetOutModel>>(environment.remoteData.apiUri + 'Account/CompanyGet', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  CompanyPut(model: GenericRequest<CompanyPutInModel>): Observable<GenericResult<CompanyPutOutModel>> {

    return this.http.post<GenericResult<CompanyPutOutModel>>(environment.remoteData.apiUri + 'Account/CompanyPut', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }




  UserCompanyList(model: GenericRequest<UserCompanyListInModel>): Observable<GenericResult<UserCompanyListOutModel>> {

    return this.http.post<GenericResult<UserCompanyListOutModel>>(environment.remoteData.apiUri + 'Account/UserCompanyList', model)
      .pipe(
        map(r => {
          //this.authService.setRole(r.data.roles);
          return r;
        }
        )
      );

  }

  UserCompanyGet(model: GenericRequest<UserCompanyGetInModel>): Observable<GenericResult<UserCompanyGetOutModel>> {

    return this.http.post<GenericResult<UserCompanyGetOutModel>>(environment.remoteData.apiUri + 'Account/UserCompanyGet', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  UserCompanyPut(model: GenericRequest<UserCompanyPutInModel>): Observable<GenericResult<UserCompanyPutOutModel>> {

    return this.http.post<GenericResult<UserCompanyPutOutModel>>(environment.remoteData.apiUri + 'Account/UserCompanyPut', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }


}
