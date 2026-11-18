import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonLabel, IonNote, IonSegment, IonSegmentButton } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

import { McpTokenListPageComponent } from './mcp-token-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonLabel, IonNote, IonSegment, IonSegmentButton,
    RouterModule.forChild([
      {
        path: '',
        component: McpTokenListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],

  declarations: [McpTokenListPageComponent],

})
export class McpTokenListPageModule { }
