import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { RequestJustificationUserPageComponent } from './request-justification-user-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: RequestJustificationUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [RequestJustificationUserPageComponent]
})
export class RequestJustificationUserPageModule { }
