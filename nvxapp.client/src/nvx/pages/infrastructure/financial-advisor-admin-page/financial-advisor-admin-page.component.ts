import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-financial-advisor-admin-page',
  templateUrl: './financial-advisor-admin-page.component.html',
  styleUrls: ['./financial-advisor-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FinancialAdvisorAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'FinancialAdvisorAdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
