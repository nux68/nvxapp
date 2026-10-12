import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-dealer-power-admin-page',
  templateUrl: './dealer-power-admin-page.component.html',
  styleUrls: ['./dealer-power-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DealerPowerAdminPageComponent {

  public title!: string;

  constructor() {
    this.title = 'DealerPowerAdminPage';
  }

  ionViewWillEnter() {
  }


}
