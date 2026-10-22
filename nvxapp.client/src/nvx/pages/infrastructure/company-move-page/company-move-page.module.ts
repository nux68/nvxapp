import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonSpinner } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CompanyMovePageComponent } from './company-move-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonSpinner,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyMovePageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CompanyMovePageComponent]
})
export class CompanyMovePageModule { }
