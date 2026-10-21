import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { LoginPageComponent } from './login-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule ,
    IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel,
    RouterModule.forChild([
      {
        path: '',
        component: LoginPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [LoginPageComponent]
})
export class LoginPageModule { }
