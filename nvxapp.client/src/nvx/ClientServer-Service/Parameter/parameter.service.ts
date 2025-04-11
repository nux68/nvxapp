import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../Utility/auth.service';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { RolesListInModel, RolesListOutModel, RolesModel } from './Models/roles-model';


@Injectable({
  providedIn: 'root'
})
export class ParameterService {


  //RolesModel

  private _roles: RolesModel[] | null = [];

  public get Roles(): RolesModel[] | null {
    return this._roles;
  }


  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  Load_Roles(model: GenericRequest<RolesListInModel>): Observable<GenericResult<RolesListOutModel>> {

    return this.http.post<GenericResult<RolesListOutModel>>(environment.remoteData.apiUri + 'Parameter/Roles', model)
      .pipe(
        map(r => {

          this._roles = r.data.roles;

          return r;
          }
        )
      );

  }



}
