import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { OrariListPageComponent } from './orari-list.component';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: OrariListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [OrariListPageComponent],

})
export class OrariListPageModule { }
