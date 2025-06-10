import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { MyTemplate1EditPageComponent } from './mytemplate1-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: MyTemplate1EditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [MyTemplate1EditPageComponent],
})
export class MyTemplate1EditPageModule { }
