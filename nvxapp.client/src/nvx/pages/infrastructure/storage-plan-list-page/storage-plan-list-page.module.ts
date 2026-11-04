import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { StoragePlanListPageComponent } from './storage-plan-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([{ path: '', component: StoragePlanListPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [StoragePlanListPageComponent],
})
export class StoragePlanListPageModule { }
