import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { UserEditPageComponent } from './user-edit-page.component';



@NgModule({
  imports: [


    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [UserEditPageComponent],

})
export class UserEditPageModule { }
