import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IonCard, IonCardContent, IonContent, IonNote } from '@ionic/angular';
import { FormuleListPageComponent } from './formule-list-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonNote,
    RouterModule.forChild([{ path: '', component: FormuleListPageComponent }]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [FormuleListPageComponent]
})
export class FormuleListPageModule { }
