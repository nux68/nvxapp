import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CustomerListPageComponent } from './customer-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([{ path: '', component: CustomerListPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CustomerListPageComponent]
})
export class CustomerListPageModule { }
