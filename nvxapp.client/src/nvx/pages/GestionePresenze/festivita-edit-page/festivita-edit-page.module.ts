import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FestivitaEditPageComponent } from './festivita-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonInput, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: FestivitaEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [FestivitaEditPageComponent]
})
export class FestivitaEditPageModule { }
