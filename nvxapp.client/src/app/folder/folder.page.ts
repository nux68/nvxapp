import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { WeatherForecastInModel, WeatherForecastModel } from '../../nvx/ClientServer-Service/WeatherForecastService/Models/weather-forecast-model';
import { WeatherForecastService } from '../../nvx/ClientServer-Service/WeatherForecastService/weather-forecast.service';
import { GenericRequest } from '../../nvx/ClientServer-Service/ModelsBase/generic-request';


@Component({
  selector: 'app-folder',
  templateUrl: './folder.page.html',
  styleUrls: ['./folder.page.scss'],
  standalone: false,
})
export class FolderPage implements OnInit {
  public folder!: string;
  private activatedRoute = inject(ActivatedRoute);

  private WeatherForecast: WeatherForecastModel[];

  constructor(private weatherForecastService: WeatherForecastService)
  { }

  ngOnInit() {
    this.folder = this.activatedRoute.snapshot.paramMap.get('id') as string;

    let request: GenericRequest<WeatherForecastInModel> = new GenericRequest<WeatherForecastInModel>();

    this.weatherForecastService.WeatherForecastGet(request).subscribe(res => {
      this.WeatherForecast = res.data.weatherForecast;
    });

  }
}
