import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { CompanyListPageComponent } from './company-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [CompanyListPageComponent],

})
export class CompanyListPageModule { }



