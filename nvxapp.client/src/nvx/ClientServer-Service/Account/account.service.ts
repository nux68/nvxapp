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
import { DealerListInModel, DealerListOutModel } from './Models/dealer-list-model';


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
                            //this.authService.setRole(r.data.roles);
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

}
