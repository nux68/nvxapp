import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCheckbox, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { AccordoSwEditPageComponent } from './accordo-sw-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonCheckbox, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea,
    RouterModule.forChild([{ path: '', component: AccordoSwEditPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [AccordoSwEditPageComponent]
})
export class AccordoSwEditPageModule { }
