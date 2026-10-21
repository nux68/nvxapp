import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow } from '@ionic/angular';
import { RouterModule, Routes } from '@angular/router';
import { CustomerListPageComponent } from './customer-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

const routes: Routes = [
  {
    path: '',
    component: CustomerListPageComponent
  }
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemSliding, IonLabel, IonRow,
    RouterModule.forChild(routes),
    SharedComponentInfrastructureModule,
  ],
  declarations: [CustomerListPageComponent]
})
export class CustomerListPageModule {}
