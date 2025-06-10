import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { AzSediListPageComponent } from './az-sedi-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: AzSediListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [AzSediListPageComponent],
})
export class AzSediListPageModule { }
