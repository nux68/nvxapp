import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonLabel, IonSegment, IonSegmentButton } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { NotificationListPageComponent } from './notification-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonLabel, IonSegment, IonSegmentButton,
    RouterModule.forChild([
      {
        path: '',
        component: NotificationListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [NotificationListPageComponent],

})
export class NotificationListPageModule { }
