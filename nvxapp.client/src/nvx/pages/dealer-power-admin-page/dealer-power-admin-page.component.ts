import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dealer-power-admin-page',
  templateUrl: './dealer-power-admin-page.component.html',
  styleUrls: ['./dealer-power-admin-page.component.scss'],
  standalone: false
})
export class DealerPowerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'DealerPowerAdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
