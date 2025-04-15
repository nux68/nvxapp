import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../../shared/shared-component.module';
import { FinancialAdvisorEditPageComponent } from './financial-advisor-edit-page.component';





@NgModule({
  imports: [
    
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorEditPageComponent
      }
    ]),
    SharedComponentModule
  ],

  declarations: [FinancialAdvisorEditPageComponent ],

})
export class FinancialAdvisorEditPageModule { }



