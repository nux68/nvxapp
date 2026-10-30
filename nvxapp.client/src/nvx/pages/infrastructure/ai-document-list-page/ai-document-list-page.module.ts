import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { AiDocumentListPageComponent } from './ai-document-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonItem, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: AiDocumentListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [AiDocumentListPageComponent],

})
export class AiDocumentListPageModule { }
