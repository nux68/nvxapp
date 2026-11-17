import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { LicenseSubscribePageComponent } from './license-subscribe-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonToggle,
    RouterModule.forChild([{ path: '', component: LicenseSubscribePageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [LicenseSubscribePageComponent],
})
export class LicenseSubscribePageModule { }
