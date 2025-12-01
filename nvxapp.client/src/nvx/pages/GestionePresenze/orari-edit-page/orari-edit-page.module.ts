import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { OrariEditPageComponent } from './orari-edit-page.component';






@NgModule({
  imports: [

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: OrariEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [OrariEditPageComponent],

})
export class OrariEditPageModule { }
