import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { ShowcaseHomePageComponent } from './showcase-home-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: ShowcaseHomePageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ShowcaseHomePageComponent]
})
export class ShowcaseHomePageModule { }
