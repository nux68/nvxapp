import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FinancialAdvisorPowerAdminPageComponent } from './financial-advisor-power-admin-page.component';


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
    ])
  ],
  declarations: [FinancialAdvisorPowerAdminPageComponent]
})
export class FinancialAdvisorPowerAdminPageModule { }


