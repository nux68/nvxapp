import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { FestivitaListPageComponent } from './festivita-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: FestivitaListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [FestivitaListPageComponent]
})
export class FestivitaListPageModule { }
