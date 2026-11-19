import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { nomeUtente } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

@Component({
  selector: 'app-breadcrumbs-impersonate',
  templateUrl: './breadcrumbs-impersonate.component.html',
  styleUrls: ['./breadcrumbs-impersonate.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class BreadcrumbsImpersonateComponent {

  // Cognome e Nome dell'anagrafica se disponibili (utenti di tipo User), altrimenti lo username
  public readonly nomeUtente = nomeUtente;

  constructor(public userNavigationService: UserNavigationService) {
  }


  navigateTo(id: string) {
    this.userNavigationService.UserGoTo(id);

    

  }
  

}
