import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TimeSheetUserPageComponent } from './time-sheet-user-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: TimeSheetUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [TimeSheetUserPageComponent]
})
export class TimeSheetUserModule { }
