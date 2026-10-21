import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { TipoSpesaEditPageComponent } from './tipo-spesa-edit-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: TipoSpesaEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TipoSpesaEditPageComponent]
})
export class TipoSpesaEditPageModule { }
