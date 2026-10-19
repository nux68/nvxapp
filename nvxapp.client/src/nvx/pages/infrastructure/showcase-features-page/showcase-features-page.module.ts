import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { ShowcaseFeaturesPageComponent } from './showcase-features-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: ShowcaseFeaturesPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ShowcaseFeaturesPageComponent]
})
export class ShowcaseFeaturesPageModule { }
