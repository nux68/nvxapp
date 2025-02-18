import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dealer-list-page',
  templateUrl: './dealer-list-page.component.html',
  styleUrls: ['./dealer-list-page.component.scss'],
  standalone: false
})
export class DealerListPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'DealerListPage';
  }

  ngOnInit() {}

}
