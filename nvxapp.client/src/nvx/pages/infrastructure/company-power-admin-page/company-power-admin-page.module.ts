import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonItem, IonLabel, IonNote } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CompanyPowerAdminPageComponent } from './company-power-admin-page.component';
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
        component: CompanyPowerAdminPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    WidgetConfigComponent
  ],
  declarations: [CompanyPowerAdminPageComponent]
})
export class CompanyPowerAdminPageModule { }
