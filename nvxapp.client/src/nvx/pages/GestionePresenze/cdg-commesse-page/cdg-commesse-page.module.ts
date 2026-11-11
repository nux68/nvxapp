import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonSpinner } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CdgCommessePageComponent } from './cdg-commesse-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonSpinner,
    RouterModule.forChild([
      {
        path: '',
        component: CdgCommessePageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CdgCommessePageComponent]
})
export class CdgCommessePageModule { }
