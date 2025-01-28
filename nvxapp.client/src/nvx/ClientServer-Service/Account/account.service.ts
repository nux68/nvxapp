import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { UserRolesInModel, UserRolesOutModel } from './Models/user-roles-model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$ = this.rolesSubject.asObservable();

  constructor(private http: HttpClient) { }

  UserRoles(model: GenericRequest<UserRolesInModel>): Observable<GenericResult<UserRolesOutModel>> {

    return this.http.post<GenericResult<UserRolesOutModel>>(environment.remoteData.apiUri + 'Account/UserRoles', model)
      .pipe(
                map(r => {
                          this.rolesSubject.next(r.data.roles);
                          return r;
                         }
                )
            );

  }


  hasRole(role: string): boolean {
    const roles = this.rolesSubject.getValue();
    return roles.includes(role);
  }

}
