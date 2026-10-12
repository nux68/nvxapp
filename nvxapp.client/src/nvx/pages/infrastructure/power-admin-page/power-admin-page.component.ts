import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-power-admin-page',
  templateUrl: './power-admin-page.component.html',
  styleUrls: ['./power-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PowerAdminPageComponent {

  public title!: string;

  constructor() {
    this.title = 'PowerAdminPage';
  }

  ionViewWillEnter() {
  }


}
