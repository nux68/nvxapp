import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { MyTemplate1Service } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/my-template1.service';
import { My_Template1Model, My_template1_GetInModel, My_template1_GetOutModel, My_template1_PutInModel, My_template1_PutOutModel } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/Models/my-template1-model';

@Component({
  selector: 'app-mytemplate1-edit-page',
  templateUrl: './mytemplate1-edit-page.component.html',
  styleUrls: ['./mytemplate1-edit-page.component.scss'],
  standalone: false
})
export class MyTemplate1EditPageComponent extends BasePageConfirmCancelComponent<My_Template1Model> {
  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private myTemplate1Service: MyTemplate1Service,
    private refresherService: RefresherService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "MyTemplate1"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  LoadData = (): Observable<My_Template1Model | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<My_template1_GetInModel> = new GenericRequest<My_template1_GetInModel>(My_template1_GetInModel);
      request.data.id = state.id;
      return this.myTemplate1Service.MyTemplate1Get(request).pipe(
        map((res) => res.data.my_Template1), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<My_Template1Model | null>((subscriber) => {
        subscriber.next(new My_Template1Model());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: My_Template1Model): Observable<boolean> => {
    let request: GenericRequest<My_template1_PutInModel> =
      new GenericRequest<My_template1_PutInModel>(My_template1_PutInModel);
    request.data.my_Template1 = editModel;
    return this.myTemplate1Service.MyTemplate1Put(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };
}
