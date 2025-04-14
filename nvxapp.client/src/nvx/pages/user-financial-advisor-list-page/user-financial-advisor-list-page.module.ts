import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../shared/shared-component.module';
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
    SharedComponentModule
  ],

  declarations: [UserFinancialAdvisorListPageComponent],

})
export class UserFinancialAdvisorListPageModule { }

