import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonLabel } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { VacationPlanPageComponent } from './vacation-plan-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonLabel,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: '',
        component: VacationPlanPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],
  declarations: [VacationPlanPageComponent]
})
export class VacationPlanPageModule { }
