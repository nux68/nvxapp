import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonRadio, IonRadioGroup, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TimbraturaQrPageComponent } from './timbratura-qr-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonItem, IonLabel, IonNote, IonRadio, IonRadioGroup, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: TimbraturaQrPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TimbraturaQrPageComponent]
})
export class TimbraturaQrPageModule { }
