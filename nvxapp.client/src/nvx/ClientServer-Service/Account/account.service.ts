import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { UserRolesInModel, UserRolesOutModel } from './Models/user-roles-model';
import { LoginInModel, LoginOutModel } from './Models/login-model';
import { AuthService } from '../../Utility/auth.service';


@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor(private http: HttpClient,
              private authService: AuthService
  ) { }

  UserRoles(model: GenericRequest<UserRolesInModel>): Observable<GenericResult<UserRolesOutModel>> {

    return this.http.post<GenericResult<UserRolesOutModel>>(environment.remoteData.apiUri + 'Account/UserRoles', model)
      .pipe(
                map(r => {
                          //this.rolesSubject.next(r.data.roles);
                  this.authService.setRole(r.data.roles);
                          return r;
                         }
                )
            );

  }

  Login(model: GenericRequest<LoginInModel>): Observable<GenericResult<LoginOutModel>> {

    return this.http.post<GenericResult<LoginOutModel>>(environment.remoteData.apiUri + 'Account/Login', model)
      .pipe(
        map(r => {
             return r;
            }
        )
      );

  }

  //hasRole(role: string): boolean {
  //  const roles = this.rolesSubject.getValue();
  //  return roles.includes(role);
  //}

}
