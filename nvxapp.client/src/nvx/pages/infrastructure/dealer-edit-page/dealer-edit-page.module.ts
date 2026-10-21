import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { DealerEditPageComponent } from './dealer-edit-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: DealerEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [DealerEditPageComponent],

})
export class DealerEditPageModule { }



