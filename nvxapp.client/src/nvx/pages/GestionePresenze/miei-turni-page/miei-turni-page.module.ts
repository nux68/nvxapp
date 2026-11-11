import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { MieiTurniPageComponent } from './miei-turni-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote,
    RouterModule.forChild([{ path: '', component: MieiTurniPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [MieiTurniPageComponent]
})
export class MieiTurniPageModule { }
