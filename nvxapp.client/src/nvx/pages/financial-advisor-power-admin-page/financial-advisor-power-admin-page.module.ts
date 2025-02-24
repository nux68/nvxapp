import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FinancialAdvisorPowerAdminPageComponent } from './financial-advisor-power-admin-page.component';
import { SharedComponentModule } from '../../shared/shared-component.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorPowerAdminPageComponent
      }
    ]),
    SharedComponentModule
  ],
  declarations: [FinancialAdvisorPowerAdminPageComponent]
})
export class FinancialAdvisorPowerAdminPageModule { }


