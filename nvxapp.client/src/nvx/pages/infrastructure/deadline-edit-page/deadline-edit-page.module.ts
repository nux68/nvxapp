import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { DeadlineEditPageComponent } from './deadline-edit-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea,
    RouterModule.forChild([
      {
        path: '',
        component: DeadlineEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [DeadlineEditPageComponent],

})
export class DeadlineEditPageModule { }
