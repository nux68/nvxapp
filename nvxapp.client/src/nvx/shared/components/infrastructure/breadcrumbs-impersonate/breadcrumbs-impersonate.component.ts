import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-breadcrumbs-impersonate',
  templateUrl: './breadcrumbs-impersonate.component.html',
  styleUrls: ['./breadcrumbs-impersonate.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class BreadcrumbsImpersonateComponent  implements OnInit {

  constructor(public userNavigationService: UserNavigationService) {
  }

  ngOnInit() {}

  navigateTo(id: string) {
    this.userNavigationService.UserGoTo(id);

    

  }
  

}
