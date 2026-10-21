import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonRow } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { MyTemplate1ListPageComponent } from './mytemplate1-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonButton, IonCard, IonCardContent, IonCol, IonContent, IonGrid, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonRow,
    RouterModule.forChild([
      {
        path: '',
        component: MyTemplate1ListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [MyTemplate1ListPageComponent],
})
export class MyTemplate1ListPageModule { }
