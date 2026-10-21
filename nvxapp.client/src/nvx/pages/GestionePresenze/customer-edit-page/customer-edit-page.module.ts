import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonInput, IonItem, IonLabel, IonNote, IonRow, IonTextarea } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { CustomerEditPageComponent } from './customer-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonInput, IonItem, IonLabel, IonNote, IonRow, IonTextarea,
    RouterModule.forChild([
      {
        path: '',
        component: CustomerEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CustomerEditPageComponent],
})
export class CustomerEditPageModule { }
