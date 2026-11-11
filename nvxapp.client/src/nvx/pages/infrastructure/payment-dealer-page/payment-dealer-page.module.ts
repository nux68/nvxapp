import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { PaymentDealerPageComponent } from './payment-dealer-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: PaymentDealerPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [PaymentDealerPageComponent],
})
export class PaymentDealerPageModule { }
