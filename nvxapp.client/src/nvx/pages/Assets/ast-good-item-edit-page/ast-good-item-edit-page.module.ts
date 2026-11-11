import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonBadge, IonButton, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { AstGoodItemEditPageComponent } from './ast-good-item-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonBadge, IonButton, IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([{ path: '', component: AstGoodItemEditPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [AstGoodItemEditPageComponent],
})
export class AstGoodItemEditPageModule { }
