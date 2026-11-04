import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonBadge, IonButton, IonButtons, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCheckbox, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentCrmModule } from '../../../shared/shared-component-crm.module';
import { CrmActivityListPageComponent } from './crm-activity-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonBadge, IonButton, IonButtons, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCheckbox, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([{ path: '', component: CrmActivityListPageComponent }]),
    SharedComponentInfrastructureModule,
    SharedComponentCrmModule
  ],
  declarations: [CrmActivityListPageComponent],
})
export class CrmActivityListPageModule { }
