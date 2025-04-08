import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../Utility/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FinancialAdvisorEditModel, FinancialAdvisorGetInModel, FinancialAdvisorPutInModel } from '../../ClientServer-Service/Account/Models/financial-advisor-model';
import { AccountService } from '../../ClientServer-Service/Account/account.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
//import { catchError, map, of } from 'rxjs';


@Component({
  selector: 'app-financial-advisor-edit-page',
  templateUrl: './financial-advisor-edit-page.component.html',
  styleUrls: ['./financial-advisor-edit-page.component.scss'],
  standalone: false
})
export class FinancialAdvisorEditPageComponent extends BasePageConfirmCancelComponent<FinancialAdvisorEditModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private accountService: AccountService)
  {

    super(navCtrl, userInterfaceService, fb);

  }

  
  get Title(): string { return "FinancialAdvisorEditPage"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      
      descrizione: [null, [Validators.required, Validators.maxLength(20)]],
      
    });
  }
    
  LoadData = (): Observable<FinancialAdvisorEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<FinancialAdvisorGetInModel> = new GenericRequest<FinancialAdvisorGetInModel>(FinancialAdvisorGetInModel);
      request.data.id = state.id;

      return this.accountService.FinancialAdvisorGet(request).pipe(
        map((res) => res.data.financialAdvisorEdit), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<FinancialAdvisorEditModel | null>((subscriber) => {
        subscriber.next(null); // Stato non valido, restituisce null
        subscriber.complete();
      });
    }
  };
  
  SaveData = (editModel: FinancialAdvisorEditModel): Observable<boolean> => {
      let request: GenericRequest<FinancialAdvisorPutInModel> = 
        new GenericRequest<FinancialAdvisorPutInModel>(FinancialAdvisorPutInModel);
      request.data.financialAdvisorEdit = editModel;

      return this.accountService.FinancialAdvisorPut(request).pipe(
        map(() => true), // Restituisce true in caso di successo
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [false]; // Restituisce false in caso di errore
        })
      );
    };


  

}


