import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { TimeClockUserPageComponent } from './time-clock-user-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: TimeClockUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [TimeClockUserPageComponent]
})
export class TimeClockUserPageModule { }
