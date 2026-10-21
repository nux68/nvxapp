import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSpinner, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CompanyMenuRestrictionPageComponent } from './company-menu-restriction-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonLabel, IonNote, IonRow, IonSpinner, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyMenuRestrictionPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CompanyMenuRestrictionPageComponent]
})
export class CompanyMenuRestrictionPageModule { }
