import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { ExportCauEditPageComponent } from './export-cau-edit-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: ExportCauEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [ExportCauEditPageComponent],

})
export class ExportCauEditPageModule { }
