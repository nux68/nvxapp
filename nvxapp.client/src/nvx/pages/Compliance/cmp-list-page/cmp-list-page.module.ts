import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSegment, IonSegmentButton, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CmpListPageComponent } from './cmp-list-page.component';

// stessa pagina per /cmp81, /cmpgdpr e /cmpstudio: l'area arriva dai data della rotta (route-compliance.service.ts)
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSegment, IonSegmentButton, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: CmpListPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CmpListPageComponent],
})
export class CmpListPageModule { }
