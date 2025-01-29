import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-power-admin-page',
  templateUrl: './power-admin-page.component.html',
  styleUrls: ['./power-admin-page.component.scss'],
  standalone: false
})
export class PowerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'PowerAdminPage';
  }

  ngOnInit() {}

}
