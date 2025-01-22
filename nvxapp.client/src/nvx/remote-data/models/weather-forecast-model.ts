
export class WeatherForecastInModel {

}

export class WeatherForecastOutModel {
  
  public weatherForecast: WeatherForecastModel[];

}

export class WeatherForecastModel {

  public Date: Date;
  public iemperatureC: number;
  public temperatureF: number;
  public summary: string;

}
