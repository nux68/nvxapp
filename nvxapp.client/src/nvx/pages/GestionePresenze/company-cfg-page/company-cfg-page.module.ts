import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CompanyCfgPageComponent } from './company-cfg-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyCfgPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [CompanyCfgPageComponent],

})
export class CompanyCfgPageModule { }
