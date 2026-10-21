import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonActionSheet, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonRow } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { TimeSheetPowerAdminPageComponent } from './time-sheet-power-admin-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonActionSheet, IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonRow,
    RouterModule.forChild([
      {
        path: '',
        component: TimeSheetPowerAdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [TimeSheetPowerAdminPageComponent]
})
export class TimeSheetPowerAdminPageModule { }
