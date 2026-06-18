import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CompanyWizardPageComponentComponent } from './company-wizard-page-component.component';


@NgModule({
  imports: [

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyWizardPageComponentComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [CompanyWizardPageComponentComponent],

})
export class CompanyWizardPageComponentModule { }