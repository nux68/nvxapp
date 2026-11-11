import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonBadge, IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle, IonContent, IonIcon, IonItem, IonLabel, IonList, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { DigestDetailPageComponent } from './digest-detail-page.component';


@NgModule({
  imports: [
    CommonModule,
    IonBadge, IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle, IonContent, IonIcon, IonItem, IonLabel, IonList, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: DigestDetailPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [DigestDetailPageComponent],

})
export class DigestDetailPageModule { }
