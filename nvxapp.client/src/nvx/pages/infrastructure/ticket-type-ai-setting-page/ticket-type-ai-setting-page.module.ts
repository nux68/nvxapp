import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonList, IonNote, IonSelect, IonSelectOption, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { TicketTypeAiSettingPageComponent } from './ticket-type-ai-setting-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonList, IonNote, IonSelect, IonSelectOption, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: TicketTypeAiSettingPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [TicketTypeAiSettingPageComponent],

})
export class TicketTypeAiSettingPageModule { }
