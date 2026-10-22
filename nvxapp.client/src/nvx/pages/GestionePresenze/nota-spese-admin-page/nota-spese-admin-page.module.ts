import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonBadge, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { NotaSpeseAdminPageComponent } from './nota-spese-admin-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonBadge, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner,
    RouterModule.forChild([
      {
        path: '',
        component: NotaSpeseAdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
  ],
  declarations: [NotaSpeseAdminPageComponent]
})
export class NotaSpeseAdminPageModule { }
