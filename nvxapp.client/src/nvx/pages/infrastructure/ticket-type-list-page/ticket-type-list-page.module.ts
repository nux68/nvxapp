import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { TicketTypeListPageComponent } from './ticket-type-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([
      {
        path: '',
        component: TicketTypeListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [TicketTypeListPageComponent],

})
export class TicketTypeListPageModule { }
