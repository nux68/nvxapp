import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-dealer-admin-page',
  templateUrl: './dealer-admin-page.component.html',
  styleUrls: ['./dealer-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DealerAdminPageComponent {

  public title!: string;

  constructor() {
    this.title = 'DealerAdminPage';
  }

  ionViewWillEnter() {
  }


}
