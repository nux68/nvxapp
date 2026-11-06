import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { UserManualPageComponent } from './user-manual-page.component';

@NgModule({
  imports: [
    CommonModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote,
    RouterModule.forChild([{ path: '', component: UserManualPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [UserManualPageComponent]
})
export class UserManualPageModule { }
