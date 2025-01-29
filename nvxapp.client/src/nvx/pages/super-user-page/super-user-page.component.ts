import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-super-user-page',
  templateUrl: './super-user-page.component.html',
  styleUrls: ['./super-user-page.component.scss'],
  standalone: false
})
export class SuperUserPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'SuperUserPage';
  }

  ngOnInit() {}

}
