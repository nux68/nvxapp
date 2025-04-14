import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../shared/shared-component.module';
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
    SharedComponentModule
  ],

  declarations: [UserEditPageComponent],

})
export class UserEditPageModule { }
