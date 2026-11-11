import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonCard, IonCardContent, IonContent, IonLabel, IonNote, IonSpinner } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { DigestListPageComponent } from './digest-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    IonCard, IonCardContent, IonContent, IonLabel, IonNote, IonSpinner,
    RouterModule.forChild([
      {
        path: '',
        component: DigestListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [DigestListPageComponent],

})
export class DigestListPageModule { }
