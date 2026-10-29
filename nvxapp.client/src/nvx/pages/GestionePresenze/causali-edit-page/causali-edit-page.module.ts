import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CausaliEditPageComponent } from './causali-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: CausaliEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [CausaliEditPageComponent]
})
export class CausaliEditPageModule { }
