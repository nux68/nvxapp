import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonBadge, IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonText, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { LicenseManagementPageComponent } from './license-management-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonBadge, IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonText, IonToggle,
    RouterModule.forChild([{ path: '', component: LicenseManagementPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [LicenseManagementPageComponent],
})
export class LicenseManagementPageModule { }
