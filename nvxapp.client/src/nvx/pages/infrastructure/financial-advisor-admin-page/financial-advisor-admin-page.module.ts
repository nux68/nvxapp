import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FinancialAdvisorAdminPageComponent } from './financial-advisor-admin-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorAdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [FinancialAdvisorAdminPageComponent]
})
export class FinancialAdvisorAdminPageModule { }


