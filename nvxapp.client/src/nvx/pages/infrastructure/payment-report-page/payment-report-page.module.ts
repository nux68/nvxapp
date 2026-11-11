import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { PaymentReportPageComponent } from './payment-report-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: PaymentReportPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [PaymentReportPageComponent],
})
export class PaymentReportPageModule { }
