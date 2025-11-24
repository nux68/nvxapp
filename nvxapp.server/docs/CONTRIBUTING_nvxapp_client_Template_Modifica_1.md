
# Modifca 1

Questo documento fornisce linee guida per creare una pagina di modifica nel client nvxapp.





- [Client](./CONTRIBUTING_nvxapp_client.md)
- [Home](./CONTRIBUTING.md)



## Componet
```linguaggio

import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Par_CausaliGetInModel, Par_CausaliModel, Par_CausaliPutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-causali-edit-page',
  templateUrl: './causali-edit-page.component.html',
  styleUrls: ['./causali-edit-page.component.scss'],
  standalone: false
})
export class CausaliEditPageComponent extends BasePageConfirmCancelComponent<Par_CausaliModel> {

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parCausaliService: ParCausaliService,
    private refresherService: RefresherService,
    private userNavigationService: UserNavigationService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "Causale"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      codice: [null, [Validators.required, Validators.maxLength(5)]],
    });
  }

  LoadData = (): Observable<Par_CausaliModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request = new GenericRequest<Par_CausaliGetInModel>(Par_CausaliGetInModel);
      request.data.id = state.id;

      return this.parCausaliService.Par_CausaliGet(request).pipe(
        map((res) => res.data.par_Causale),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Par_CausaliModel | null>((subscriber) => {
        subscriber.next(new Par_CausaliModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_CausaliModel): Observable<boolean> => {
    let request = new GenericRequest<Par_CausaliPutInModel>(Par_CausaliPutInModel);
    request.data.par_Causale = editModel;

    return this.parCausaliService.Par_CausaliPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };
}



```



## HTML
```linguaggio

<app-page-toolbar [title]="Title"
                  [showBreadcrumbs]="true">
</app-page-toolbar>

<ion-content [fullscreen]="true">
  <ion-card>
    <ion-card-content>

      <ion-item class="nvx-item-4-title">
        <ion-label>Info</ion-label>
      </ion-item>

      <form [formGroup]="_editForm">


        <ion-item>
          <ion-label position="floating">Codice</ion-label>
          <ion-input type="text" formControlName="codice"></ion-input>
        </ion-item>
        <ion-note color="danger" *ngIf="_editForm.get('codice')?.hasError('required') && _editForm.get('codice')?.touched">
          Codice è obbligatorio.
        </ion-note>
        <ion-note color="danger" *ngIf="_editForm.get('codice')?.hasError('maxlength') && _editForm.get('codice')?.touched">
          Codice non può superare i 5 caratteri.
        </ion-note>


        <ion-item>
          <ion-label position="floating">Descrizione</ion-label>
          <ion-input type="text" formControlName="descrizione"></ion-input>
        </ion-item>
        <ion-note color="danger" *ngIf="_editForm.get('descrizione')?.hasError('required') && _editForm.get('descrizione')?.touched">
          Descrizione è obbligatorio.
        </ion-note>
        <ion-note color="danger" *ngIf="_editForm.get('descrizione')?.hasError('maxlength') && _editForm.get('descrizione')?.touched">
          Descrizione non può superare i 50 caratteri.
        </ion-note>


      </form>

    </ion-card-content>
  </ion-card>
</ion-content>

<app-page-buttonbar [buttonbar]="buttonbar"></app-page-buttonbar>



```




## CSS
```linguaggio

ion-menu-button {
  color: var(--ion-color-primary);
}

#container {
  text-align: center;
  position: absolute;
  left: 0;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
}

#container strong {
  font-size: 20px;
  line-height: 26px;
}

#container p {
  font-size: 16px;
  line-height: 22px;
  color: #8c8c8c;
  margin: 0;
}

#container a {
  text-decoration: none;
}



```


## MODULE
```linguaggio

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CausaliEditPageComponent } from './causali-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CausaliEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [CausaliEditPageComponent]
})
export class CausaliEditPageModule { }


```