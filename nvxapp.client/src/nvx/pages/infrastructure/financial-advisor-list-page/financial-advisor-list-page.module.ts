import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FinancialAdvisorListPageComponent } from './financial-advisor-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [FinancialAdvisorListPageComponent]
})
export class FinancialAdvisorListPageModule { }


