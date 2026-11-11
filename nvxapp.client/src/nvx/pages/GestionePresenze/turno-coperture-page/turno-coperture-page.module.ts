import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCheckbox, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TurnoCoperturePageComponent } from './turno-coperture-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonCheckbox, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: TurnoCoperturePageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TurnoCoperturePageComponent]
})
export class TurnoCoperturePageModule { }
