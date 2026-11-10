import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { UserPageComponent } from './user-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { WidgetConfigComponent } from '../../../shared/components/infrastructure/widgets/widget-config/widget-config.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote,
    RouterModule.forChild([
      {
        path: '',
        component: UserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    WidgetConfigComponent
  ],
  declarations: [UserPageComponent]
})
export class UserPageModule { }
