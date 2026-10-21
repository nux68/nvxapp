import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonContent, IonIcon, IonInput, IonItem } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { ForgotPasswordPageComponent } from './forgot-password-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonContent, IonIcon, IonInput, IonItem,
    RouterModule.forChild([
      {
        path: '',
        component: ForgotPasswordPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ForgotPasswordPageComponent]
})
export class ForgotPasswordPageModule { }
