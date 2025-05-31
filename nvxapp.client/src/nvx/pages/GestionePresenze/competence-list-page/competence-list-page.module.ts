import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CompetenceListPageComponent } from './competence-list-page.component';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompetenceListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [CompetenceListPageComponent],

})
export class CompetenceListPageModule { }
