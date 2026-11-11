import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TimbraturaSedePageComponent } from './timbratura-sede-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle,
    RouterModule.forChild([{ path: '', component: TimbraturaSedePageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TimbraturaSedePageComponent]
})
export class TimbraturaSedePageModule { }
