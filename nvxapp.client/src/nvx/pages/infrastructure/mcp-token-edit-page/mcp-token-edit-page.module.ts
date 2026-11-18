import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { McpTokenEditPageComponent } from './mcp-token-edit-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonContent, IonIcon, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption,
    RouterModule.forChild([{ path: '', component: McpTokenEditPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [McpTokenEditPageComponent],
})
export class McpTokenEditPageModule { }
