import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonLabel, IonRow, IonSegment, IonSegmentButton, IonSpinner, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { AnomaliePageComponent } from './anomalie-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonLabel, IonRow, IonSegment, IonSegmentButton, IonSpinner, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: AnomaliePageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [AnomaliePageComponent]
})
export class AnomaliePageModule { }
