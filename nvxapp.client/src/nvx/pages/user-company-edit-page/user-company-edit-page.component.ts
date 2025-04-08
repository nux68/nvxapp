import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { UserCompanyEditModel, UserCompanyGetInModel, UserCompanyPutInModel } from '../../ClientServer-Service/Account/Models/user-company-model';


@Component({
  selector: 'app-user-company-edit-page',
  templateUrl: './user-company-edit-page.component.html',
  styleUrls: ['./user-company-edit-page.component.scss'],
  standalone:false
})
export class UserCompanyEditPageComponent extends BasePageConfirmCancelComponent<UserCompanyEditModel> {

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "UserCompanyEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],

    });
  }

  LoadData = (): Observable<UserCompanyEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<UserCompanyGetInModel> = new GenericRequest<UserCompanyGetInModel>(UserCompanyGetInModel);
      request.data.id = state.id;

      return this.accountService.UserCompanyGet(request).pipe(
        map((res) => res.data.userCompanyEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<UserCompanyEditModel | null>((subscriber) => {
        subscriber.next(null); // Stato non valido, restituisce null
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: UserCompanyEditModel): Observable<boolean> => {
    let request: GenericRequest<UserCompanyPutInModel> =
      new GenericRequest<UserCompanyPutInModel>(UserCompanyPutInModel);
    request.data.userCompanyEdit = editModel;

    return this.accountService.UserCompanyPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };




}
