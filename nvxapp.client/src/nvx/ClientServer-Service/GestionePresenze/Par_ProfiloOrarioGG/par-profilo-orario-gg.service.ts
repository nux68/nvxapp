import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {Par_ProfiloOrarioGG_Get_4Edit_InModel,Par_ProfiloOrarioGG_Get_4Edit_OutModel,Par_ProfiloOrarioGG_Put_4Edit_InModel,Par_ProfiloOrarioGG_Put_4Edit_OutModel} from './Models/par-profilo-orario-gg-model';

@Injectable({
  providedIn: 'root'
})
export class ParProfiloOrarioGGService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  Par_ProfiloOrarioGG_Get(model: GenericRequest<Par_ProfiloOrarioGG_Get_4Edit_InModel>): Observable<GenericResult<Par_ProfiloOrarioGG_Get_4Edit_OutModel>> {
    return this.http.post<GenericResult<Par_ProfiloOrarioGG_Get_4Edit_OutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrarioGG/Par_ProfiloOrarioGG_Get', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_ProfiloOrarioGG_Put(model: GenericRequest<Par_ProfiloOrarioGG_Put_4Edit_InModel>): Observable<GenericResult<Par_ProfiloOrarioGG_Put_4Edit_OutModel>> {
    return this.http.post<GenericResult<Par_ProfiloOrarioGG_Put_4Edit_OutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrarioGG/Par_ProfiloOrarioGG_Put', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
}
