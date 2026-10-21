import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonButton, IonContent, IonIcon } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { ShowcaseAboutPageComponent } from './showcase-about-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    IonButton, IonContent, IonIcon,
    RouterModule.forChild([
      {
        path: '',
        component: ShowcaseAboutPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ShowcaseAboutPageComponent]
})
export class ShowcaseAboutPageModule { }
