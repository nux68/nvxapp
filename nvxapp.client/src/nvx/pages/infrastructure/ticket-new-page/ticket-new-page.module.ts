import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonNote, IonSelect, IonSelectOption, IonTextarea } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { TicketNewPageComponent } from './ticket-new-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonNote, IonSelect, IonSelectOption, IonTextarea,
    RouterModule.forChild([
      {
        path: '',
        component: TicketNewPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [TicketNewPageComponent],

})
export class TicketNewPageModule { }
