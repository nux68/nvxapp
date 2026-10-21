import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FinancialAdvisorPowerAdminPageComponent } from './financial-advisor-power-admin-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorPowerAdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [FinancialAdvisorPowerAdminPageComponent]
})
export class FinancialAdvisorPowerAdminPageModule { }


