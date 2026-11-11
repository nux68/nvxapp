import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { LavoroAgilePageComponent } from './lavoro-agile-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonNote,
    RouterModule.forChild([{ path: '', component: LavoroAgilePageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [LavoroAgilePageComponent]
})
export class LavoroAgilePageModule { }
