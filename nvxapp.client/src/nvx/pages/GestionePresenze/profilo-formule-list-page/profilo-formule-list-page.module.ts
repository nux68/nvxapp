import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IonCard, IonCardContent, IonContent } from '@ionic/angular';
import { ProfiloFormuleListPageComponent } from './profilo-formule-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent,
    RouterModule.forChild([{ path: '', component: ProfiloFormuleListPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [ProfiloFormuleListPageComponent]
})
export class ProfiloFormuleListPageModule { }
