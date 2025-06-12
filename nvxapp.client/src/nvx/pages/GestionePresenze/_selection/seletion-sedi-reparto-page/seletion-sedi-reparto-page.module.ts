import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../../shared/shared-component-infrastructure.module';
import { SeletionSediRepartoPageComponent } from './seletion-sedi-reparto-page.component';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: SeletionSediRepartoPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [SeletionSediRepartoPageComponent]
})
export class SeletionSediRepartoPageModule { }
