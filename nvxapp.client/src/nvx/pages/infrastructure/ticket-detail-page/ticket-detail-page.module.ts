import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle, IonChip, IonContent, IonIcon, IonItem, IonLabel, IonList, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { TicketDetailPageComponent } from './ticket-detail-page.component';


@NgModule({
  imports: [
    CommonModule,
    IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle, IonChip, IonContent, IonIcon, IonItem, IonLabel, IonList, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: TicketDetailPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [TicketDetailPageComponent],

})
export class TicketDetailPageModule { }
