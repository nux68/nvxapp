import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-company-power-admin-page',
  templateUrl: './company-power-admin-page.component.html',
  styleUrls: ['./company-power-admin-page.component.scss'],
  standalone: false
})
export class CompanyPowerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'CompanyPowerAdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
