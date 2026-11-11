import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { PaymentDealerConfigPageComponent } from './payment-dealer-config-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote,
    RouterModule.forChild([{ path: '', component: PaymentDealerConfigPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [PaymentDealerConfigPageComponent],
})
export class PaymentDealerConfigPageModule { }
