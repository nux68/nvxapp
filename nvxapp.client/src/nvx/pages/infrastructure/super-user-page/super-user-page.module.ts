import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonContent } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SuperUserPageComponent } from './super-user-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonContent,
    RouterModule.forChild([
      {
        path: '',
        component: SuperUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [SuperUserPageComponent]
})
export class SuperUserPageModule { }
