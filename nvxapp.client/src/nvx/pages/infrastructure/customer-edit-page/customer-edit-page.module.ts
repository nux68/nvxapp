import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonRow, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { CustomerEditPageComponent } from './customer-edit-page.component';
import { CustomerContactDialogComponent } from './customer-contact-dialog/customer-contact-dialog.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonRow, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([{ path: '', component: CustomerEditPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CustomerEditPageComponent, CustomerContactDialogComponent],
})
export class CustomerEditPageModule { }
