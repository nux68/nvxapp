import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { TicketTypeEditPageComponent } from './ticket-type-edit-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: TicketTypeEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [TicketTypeEditPageComponent],

})
export class TicketTypeEditPageModule { }
