import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
// FormsModule già incluso: necessario per [(ngModel)] su ion-segment
import { IonBadge, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { ActivityStatisticsPageComponent } from './activity-statistics-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonBadge, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: '',
        component: ActivityStatisticsPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [ActivityStatisticsPageComponent]
})
export class ActivityStatisticsPageModule { }
