import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false
})
export class UserPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'UserPage';
  }

  ngOnInit() {}

}
