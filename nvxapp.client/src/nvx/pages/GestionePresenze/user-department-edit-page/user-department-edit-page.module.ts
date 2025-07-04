import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { UserDepartmentEditPageComponent } from './user-department-edit-page.component';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserDepartmentEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [UserDepartmentEditPageComponent],
})
export class UserDepartmentEditPageModule {}
