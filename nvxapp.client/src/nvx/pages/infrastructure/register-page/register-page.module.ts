import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCheckbox, IonContent, IonIcon, IonInput, IonItem, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { RegisterPageComponent } from './register-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCheckbox, IonContent, IonIcon, IonInput, IonItem, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: RegisterPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [RegisterPageComponent]
})
export class RegisterPageModule { }
