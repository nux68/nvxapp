import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonBadge, IonCard, IonCardContent, IonCheckbox, IonContent, IonItem, IonLabel, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { DeadlineListPageComponent } from './deadline-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonBadge, IonCard, IonCardContent, IonCheckbox, IonContent, IonItem, IonLabel, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: DeadlineListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [DeadlineListPageComponent],

})
export class DeadlineListPageModule { }
