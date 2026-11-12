import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCol, IonContent, IonItem, IonLabel, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { RequestListPageComponent } from './request-list-page.component';


// "Le mie richieste" (/requests) e "Richieste da approvare" (/requestsadmin): ANALISI_INFRASTRUCTURE_RICHIESTE.md
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonCol, IonContent, IonItem, IonLabel, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: RequestListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [RequestListPageComponent],

})
export class RequestListPageModule { }
