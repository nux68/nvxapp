import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { CommessaEditPageComponent } from './commessa-edit-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([
      {
        path: '',
        component: CommessaEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [CommessaEditPageComponent],
})
export class CommessaEditPageModule { }
