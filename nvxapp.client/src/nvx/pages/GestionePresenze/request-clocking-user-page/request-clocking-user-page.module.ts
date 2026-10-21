import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { RequestClockingUserPageComponent } from './request-clocking-user-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: RequestClockingUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [RequestClockingUserPageComponent]
})
export class RequestClockingUserPageModule { }
