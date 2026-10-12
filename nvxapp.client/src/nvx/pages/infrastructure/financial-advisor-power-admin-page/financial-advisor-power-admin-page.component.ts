import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-financial-advisor-power-admin-page',
  templateUrl: './financial-advisor-power-admin-page.component.html',
  styleUrls: ['./financial-advisor-power-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FinancialAdvisorPowerAdminPageComponent {

  public title!: string;

  constructor() {
    this.title = 'FinancialAdvisorPowerAdminPage';
  }

  ionViewWillEnter() {
  }


}
