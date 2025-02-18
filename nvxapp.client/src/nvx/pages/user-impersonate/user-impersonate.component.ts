import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../Utility/user-navigation.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-impersonate',
  templateUrl: './user-impersonate.component.html',
  styleUrls: ['./user-impersonate.component.scss'],
  standalone: false
})
export class UserImpersonateComponent  implements OnInit {

  public title!: string;
  userGoBackForm: FormGroup;

  constructor(public userNavigationService: UserNavigationService,
              private fb: FormBuilder,) {

    this.title = 'UserImpersonate';

    this.userGoBackForm = this.fb.group({
      //email: ['', [Validators.required, Validators.email]],
      //email: ['', [Validators.required, Validators.minLength(3)]],
      //password: ['', [Validators.required, Validators.minLength(3)]]
    });

  }

  ngOnInit() {}

  UserGoBack() {
    this.userNavigationService.UserGoBack();
  }

}
