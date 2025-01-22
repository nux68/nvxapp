import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { WeatherForecastService } from '../../nvx/remote-data/weather-forecast.service';
import { GenericRequestModel } from '../../nvx/remote-data/models/generic-request-model';
import { WeatherForecastInModel, WeatherForecastOutModel,WeatherForecastModel } from '../../nvx/remote-data/models/weather-forecast-model';

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

    let request: GenericRequestModel<WeatherForecastInModel> = new GenericRequestModel<WeatherForecastInModel>();

    this.weatherForecastService.WeatherForecastGet(request).subscribe(res => {
      this.WeatherForecast = res.data.weatherForecast;
    });

  }
}
