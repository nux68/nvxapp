
export class WeatherForecastModel {

  public date: Date;
  public iemperatureC: number;
  public temperatureF: number;
  public summary: string;

}


export class WeatherForecastInModel {

}

export class WeatherForecastOutModel {
  
  public weatherForecast: WeatherForecastModel[];

}


