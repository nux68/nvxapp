import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { TicketListPageComponent } from './ticket-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: TicketListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [TicketListPageComponent],

})
export class TicketListPageModule { }
