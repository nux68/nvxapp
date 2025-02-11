import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dealer-admin-page',
  templateUrl: './dealer-admin-page.component.html',
  styleUrls: ['./dealer-admin-page.component.scss'],
  standalone: false
})
export class DealerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'DealerAdminPage';
  }

  ngOnInit() {}

}
