import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { ShowcaseContactPageComponent } from './showcase-contact-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedShowcaseModule } from '../../../shared/shared-showcase.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: ShowcaseContactPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedShowcaseModule
  ],
  declarations: [ShowcaseContactPageComponent]
})
export class ShowcaseContactPageModule { }
