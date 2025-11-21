import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CausaliListPageComponent } from './causali-list-page.component';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CausaliListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [CausaliListPageComponent]
})
export class CausaliListPageModule {}
