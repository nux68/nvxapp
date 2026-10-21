import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { CompetenceEditPageComponent } from './competence-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: CompetenceEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CompetenceEditPageComponent],
})
export class CompetenceEditPageModule { }
