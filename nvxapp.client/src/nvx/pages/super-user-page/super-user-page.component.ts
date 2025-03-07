import { Component, OnInit } from '@angular/core';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { UserRolesInModel } from '../../ClientServer-Service/Account/Models/user-roles-model';
import { AccountService } from '../../ClientServer-Service/Account/account.service';

@Component({
  selector: 'app-super-user-page',
  templateUrl: './super-user-page.component.html',
  styleUrls: ['./super-user-page.component.scss'],
  standalone: false
})
export class SuperUserPageComponent  implements OnInit {

  public title!: string;

  constructor(private accountService: AccountService
  ) {
    this.title = 'SuperUserPage';
  }

  ionViewWillEnter() {
    let request: GenericRequest<UserRolesInModel> = new GenericRequest<UserRolesInModel>(UserRolesInModel);
    this.accountService.UserRoles(request).subscribe(x => {

    });
  }

  ngOnInit() {}

}
