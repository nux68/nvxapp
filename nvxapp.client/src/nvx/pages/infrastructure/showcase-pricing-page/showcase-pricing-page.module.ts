import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonAccordion, IonAccordionGroup, IonButton, IonContent, IonIcon, IonItem, IonLabel } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { ShowcasePricingPageComponent } from './showcase-pricing-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    IonAccordion, IonAccordionGroup, IonButton, IonContent, IonIcon, IonItem, IonLabel,
    RouterModule.forChild([
      {
        path: '',
        component: ShowcasePricingPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ShowcasePricingPageComponent]
})
export class ShowcasePricingPageModule { }
