import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { AdminPageComponent } from './admin-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonContent,
    RouterModule.forChild([
      {
        path: '',
        component: AdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [AdminPageComponent]
})
export class AdminPageModule { }
