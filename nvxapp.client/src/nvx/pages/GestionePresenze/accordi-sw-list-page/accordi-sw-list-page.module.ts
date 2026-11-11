import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { AccordiSwListPageComponent } from './accordi-sw-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([{ path: '', component: AccordiSwListPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [AccordiSwListPageComponent]
})
export class AccordiSwListPageModule { }
