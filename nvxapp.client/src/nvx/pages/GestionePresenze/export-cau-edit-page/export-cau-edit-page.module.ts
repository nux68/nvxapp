import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { ExportCauEditPageComponent } from './export-cau-edit-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea,
    RouterModule.forChild([
      {
        path: '',
        component: ExportCauEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],

  declarations: [ExportCauEditPageComponent],

})
export class ExportCauEditPageModule { }
