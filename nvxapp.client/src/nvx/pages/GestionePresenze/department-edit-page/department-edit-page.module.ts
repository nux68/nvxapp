import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonReorder, IonReorderGroup, IonRow, IonSegment, IonSegmentButton } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { DepartmentEditPageComponent } from './department-edit-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonInput, IonItem, IonItemSliding, IonLabel, IonNote, IonReorder, IonReorderGroup, IonRow, IonSegment, IonSegmentButton,
    RouterModule.forChild([
      {
        path: '',
        component: DepartmentEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
  ],

  declarations: [DepartmentEditPageComponent],

})
export class DepartmentEditPageModule { }
