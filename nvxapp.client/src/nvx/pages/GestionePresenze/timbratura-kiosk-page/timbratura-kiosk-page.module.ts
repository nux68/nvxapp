import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TimbraturaKioskPageComponent } from './timbratura-kiosk-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: TimbraturaKioskPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TimbraturaKioskPageComponent]
})
export class TimbraturaKioskPageModule { }
