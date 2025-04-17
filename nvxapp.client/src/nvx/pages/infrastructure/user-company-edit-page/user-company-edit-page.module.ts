import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { UserCompanyEditPageComponent } from './user-company-edit-page.component';





@NgModule({
  imports: [
    
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserCompanyEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [UserCompanyEditPageComponent],

})
export class UserCompanyEditPageModule { }
