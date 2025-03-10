import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../Utility/auth.service';
import { ChatAIInModel, ChatAIOutModel } from './Models/chat-AI-model';

@Injectable({
  providedIn: 'root'
})
export class ChatAIService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  SendMessage(model: GenericRequest<ChatAIInModel>): Observable<GenericResult<ChatAIOutModel>> {

    return this.http.post<GenericResult<ChatAIOutModel>>(environment.remoteData.apiUri + 'ChatAI/SendMessage', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
