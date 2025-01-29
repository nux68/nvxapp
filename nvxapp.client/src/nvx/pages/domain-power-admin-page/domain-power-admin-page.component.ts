import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-domain-power-admin-page',
  templateUrl: './domain-power-admin-page.component.html',
  styleUrls: ['./domain-power-admin-page.component.scss'],
  standalone: false
})
export class DomainPowerAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'DomainPowerAdminPage';
  }

  ngOnInit() {}

}
