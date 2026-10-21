import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonModal, IonNote, IonRange, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonToggle } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { NgModule } from '@angular/core';
import { ProfiloOrarioEditPageComponent } from './profilo-orario-edit-page.component';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';






@NgModule({
  imports: [

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonModal, IonNote, IonRange, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: ProfiloOrarioEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule
  ],

  declarations: [ProfiloOrarioEditPageComponent],

})
export class ProfiloOrarioEditPageModule { }
