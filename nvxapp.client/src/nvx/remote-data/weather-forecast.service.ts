import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericRequestModel } from './models/generic-request-model';
import { GenericResult } from './models/generic-result-model';
import { WeatherForecastInModel, WeatherForecastOutModel } from './models/weather-forecast-model';

@Injectable({
  providedIn: 'root'
})
export class WeatherForecastService {

  constructor(private http: HttpClient) {
  }

  WeatherForecastGet(model: GenericRequestModel<WeatherForecastInModel>): Observable<GenericResult<WeatherForecastOutModel>> {

    return this.http.post<GenericResult<WeatherForecastOutModel>>(environment.remoteData.apiUri + 'WeatherForecast/Get', model)
      //.pipe(
      //map(r => {
      //  //r.data.weatherForecast
      //  //let retVal = r;
      //  // retVal.data.customerNote = [...r.data.customerNote].map(j => new CustomerNoteModel().fromInterface(j));
      //  return r;
      //}
      //)
    //)
      ;

  }


}
