import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  standalone: false
})
export class LoginPageComponent  implements OnInit {

  public title!: string;

  constructor() {
    this.title = 'Login';
  }

  ngOnInit() {}

}
