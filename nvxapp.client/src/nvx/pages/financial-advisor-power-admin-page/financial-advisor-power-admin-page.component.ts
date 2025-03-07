import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-financial-advisor-power-admin-page',
  templateUrl: './financial-advisor-power-admin-page.component.html',
  styleUrls: ['./financial-advisor-power-admin-page.component.scss'],
  standalone: false
})
export class FinancialAdvisorPowerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'FinancialAdvisorPowerAdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
