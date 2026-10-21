import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonIcon } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { LogoutPageComponent } from './logout-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonIcon,
    RouterModule.forChild([
      {
        path: '',
        component: LogoutPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [LogoutPageComponent]
})
export class LogoutPageModule { }
