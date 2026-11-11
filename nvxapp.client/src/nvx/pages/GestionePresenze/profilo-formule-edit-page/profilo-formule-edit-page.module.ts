import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { ProfiloFormuleEditPageComponent } from './profilo-formule-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote,
    RouterModule.forChild([{ path: '', component: ProfiloFormuleEditPageComponent }]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
  ],
  declarations: [ProfiloFormuleEditPageComponent]
})
export class ProfiloFormuleEditPageModule { }
