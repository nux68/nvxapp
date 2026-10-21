import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserPageComponent } from './user-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel,
    RouterModule.forChild([
      {
        path: '',
        component: UserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [UserPageComponent]
})
export class UserPageModule { }


