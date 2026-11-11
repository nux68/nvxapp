import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonList, IonListHeader, IonNote, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { PianificazioneTurniPageComponent } from './pianificazione-turni-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonList, IonListHeader, IonNote, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: PianificazioneTurniPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [PianificazioneTurniPageComponent]
})
export class PianificazioneTurniPageModule { }
