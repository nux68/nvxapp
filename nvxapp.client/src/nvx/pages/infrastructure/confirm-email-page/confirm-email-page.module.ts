import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonContent, IonIcon, IonItem, IonLabel, IonList, IonSpinner } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { ConfirmEmailPageComponent } from './confirm-email-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    IonContent, IonIcon, IonItem, IonLabel, IonList, IonSpinner,
    RouterModule.forChild([
      {
        path: '',
        component: ConfirmEmailPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ConfirmEmailPageComponent]
})
export class ConfirmEmailPageModule { }
