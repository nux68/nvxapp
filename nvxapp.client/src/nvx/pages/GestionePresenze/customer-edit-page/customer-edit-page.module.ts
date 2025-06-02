import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { CustomerEditPageComponent } from './customer-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CustomerEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CustomerEditPageComponent],
})
export class CustomerEditPageModule { }
