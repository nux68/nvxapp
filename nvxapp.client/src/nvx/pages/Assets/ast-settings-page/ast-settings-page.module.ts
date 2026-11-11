import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { AstSettingsPageComponent } from './ast-settings-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonTextarea, IonToggle,
    RouterModule.forChild([{ path: '', component: AstSettingsPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [AstSettingsPageComponent],
})
export class AstSettingsPageModule { }
