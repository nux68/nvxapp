import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-domain-admin-page',
  templateUrl: './domain-admin-page.component.html',
  styleUrls: ['./domain-admin-page.component.scss'],
  standalone: false
})
export class DomainAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'DomainAdminPage';
  }

  ngOnInit() {}

}
