import { Injectable } from '@angular/core';
import { catchError, of, retry, tap, timer } from 'rxjs';
import { GenericRequest } from '../ClientServer-Service/ModelsBase/generic-request';

@Injectable({
  providedIn: 'root'
})
export class SharedParameterGestionePresenzeService {

  private _isLoad = false;
  get IsLoad() { return this._isLoad; }
  set IsLoad(newValue) { this._isLoad = newValue; }



  constructor() {}

  

  public InitCall(updateProgress: (calls: any[]) => void): any[] {
    let calls: any[] = [];

    return calls;
  }

}
