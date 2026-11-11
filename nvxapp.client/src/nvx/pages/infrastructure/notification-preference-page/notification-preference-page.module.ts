import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonNote, IonRow, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NotificationPreferencePageComponent } from './notification-preference-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonNote, IonRow, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: NotificationPreferencePageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [NotificationPreferencePageComponent],

})
export class NotificationPreferencePageModule { }
