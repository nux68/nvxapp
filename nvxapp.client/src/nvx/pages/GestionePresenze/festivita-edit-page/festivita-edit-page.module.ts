import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { FestivitaEditPageComponent } from './festivita-edit-page.component';
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
        component: FestivitaEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [FestivitaEditPageComponent]
})
export class FestivitaEditPageModule { }
