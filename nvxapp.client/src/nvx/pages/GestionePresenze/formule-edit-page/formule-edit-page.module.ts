import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import {
  IonBadge, IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList,
  IonNote, IonProgressBar, IonRow, IonSearchbar, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea
} from '@ionic/angular';
import { FormuleEditPageComponent } from './formule-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonBadge, IonButton, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList,
    IonNote, IonProgressBar, IonRow, IonSearchbar, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea,
    RouterModule.forChild([{ path: '', component: FormuleEditPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [FormuleEditPageComponent]
})
export class FormuleEditPageModule { }
