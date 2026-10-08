import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-company-admin-page',
  templateUrl: './company-admin-page.component.html',
  styleUrls: ['./company-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyAdminPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'CompanyAdminPage';
  }

  ionViewWillEnter() {
  }

  ngOnInit() {}

}
