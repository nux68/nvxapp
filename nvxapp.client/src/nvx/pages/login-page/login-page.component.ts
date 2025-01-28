import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserRolesInModel } from '../../ClientServer-Service/Account/Models/user-roles-model';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
  standalone: false
})
export class LoginPageComponent  implements OnInit {

  public title!: string;

  constructor(private accountService: AccountService) {
    this.title = 'Login';

    

  }

  ngOnInit() {

    let request: GenericRequest<UserRolesInModel> = new GenericRequest<UserRolesInModel>();
    this.accountService.UserRoles(request).subscribe(x => {

      var c = 0;

    });

  }

}
