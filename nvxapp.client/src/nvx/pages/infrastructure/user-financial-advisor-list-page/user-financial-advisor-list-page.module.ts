import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { UserFinancialAdvisorListPageComponent } from './user-financial-advisor-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserFinancialAdvisorListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [UserFinancialAdvisorListPageComponent],

})
export class UserFinancialAdvisorListPageModule { }

