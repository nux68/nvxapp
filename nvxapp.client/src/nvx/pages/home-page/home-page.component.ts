import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss'],
  standalone: false
})
export class HomePageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'Home';
  }

  ngOnInit() {}

}
