import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-company-power-admin-page',
  templateUrl: './company-power-admin-page.component.html',
  styleUrls: ['./company-power-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyPowerAdminPageComponent {

  public title!: string;

  constructor() {
    this.title = 'CompanyPowerAdminPage';
  }

  ionViewWillEnter() {
  }


}
