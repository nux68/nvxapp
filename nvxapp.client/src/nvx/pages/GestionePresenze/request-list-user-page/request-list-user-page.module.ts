import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { RequestListUserPageComponent } from './request-list-user-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: RequestListUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [RequestListUserPageComponent],

})
export class RequestListUserPageModule { }
