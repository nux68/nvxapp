import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { JustificationEditPageComponent } from './justification-edit-page.component';


@NgModule({
  imports: [

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: JustificationEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [JustificationEditPageComponent],

})
export class JustificationEditPageModule { }
