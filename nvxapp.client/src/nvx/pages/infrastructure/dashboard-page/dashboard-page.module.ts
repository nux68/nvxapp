import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { DashboardPageComponent } from './dashboard-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DashboardPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [DashboardPageComponent]
})
export class DashboardPageModule { }
