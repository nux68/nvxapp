import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { AiDocumentEditPageComponent } from './ai-document-edit-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: AiDocumentEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [AiDocumentEditPageComponent],

})
export class AiDocumentEditPageModule { }
