import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonBadge, IonButton, IonButtons, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonFooter, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner, IonTitle, IonToolbar } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { NotaSpeseAdminPageComponent } from './nota-spese-admin-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonBadge, IonButton, IonButtons, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonFooter, IonGrid, IonHeader, IonIcon, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner, IonTitle, IonToolbar,
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
