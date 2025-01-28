import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-logout-page',
  templateUrl: './logout-page.component.html',
  styleUrls: ['./logout-page.component.scss'],
  standalone: false
})
export class LogoutPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'Logout';
  }

  ngOnInit() {}

}
