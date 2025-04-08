import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { DealerEditModel, DealerGetInModel, DealerListInModel, DealerPutInModel } from '../../ClientServer-Service/Account/Models/dealer-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, catchError } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';


@Component({
  selector: 'app-dealer-edit-page',
  templateUrl: './dealer-edit-page.component.html',
  styleUrls: ['./dealer-edit-page.component.scss'],
  standalone: false
}) 
export class DealerEditPageComponent extends BasePageConfirmCancelComponent<DealerEditModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private accountService: AccountService) {

    super(navCtrl, userInterfaceService, fb);

  }
    

  get Title(): string { return "DealerEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(20)]],

    });
  }

  LoadData = (): Observable<DealerEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<DealerGetInModel> = new GenericRequest<DealerGetInModel>(DealerGetInModel);
      request.data.id = state.id;

      return this.accountService.DealerGet(request).pipe(
        map((res) => res.data.dealerEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<DealerEditModel | null>((subscriber) => {
        subscriber.next(null); // Stato non valido, restituisce null
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: DealerEditModel): Observable<boolean> => {
    let request: GenericRequest<DealerPutInModel> =
      new GenericRequest<DealerPutInModel>(DealerPutInModel);
    request.data.dealerEdit = editModel;

    return this.accountService.DealerPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };




}
