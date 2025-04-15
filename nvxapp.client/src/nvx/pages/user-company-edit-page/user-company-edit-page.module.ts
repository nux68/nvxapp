import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../shared/shared-component.module';
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
    SharedComponentModule
  ],

  declarations: [UserCompanyEditPageComponent],

})
export class UserCompanyEditPageModule { }
