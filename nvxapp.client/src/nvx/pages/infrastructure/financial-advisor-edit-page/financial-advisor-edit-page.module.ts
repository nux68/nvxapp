import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { FinancialAdvisorEditPageComponent } from './financial-advisor-edit-page.component';





@NgModule({
  imports: [
    
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: FinancialAdvisorEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [FinancialAdvisorEditPageComponent ],

})
export class FinancialAdvisorEditPageModule { }



