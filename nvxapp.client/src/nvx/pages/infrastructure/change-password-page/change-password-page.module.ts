import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonContent, IonIcon, IonInput, IonItem, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { ChangePasswordPageComponent } from './change-password-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonContent, IonIcon, IonInput, IonItem, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: ChangePasswordPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ChangePasswordPageComponent]
})
export class ChangePasswordPageModule { }
