import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { DepartmentListPageComponent } from './department-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DepartmentListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [DepartmentListPageComponent],

})
export class DepartmentListPageModule { }
