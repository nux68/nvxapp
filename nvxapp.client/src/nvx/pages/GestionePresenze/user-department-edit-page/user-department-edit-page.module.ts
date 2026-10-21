import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner, IonToggle } from '@ionic/angular';
import { UserDepartmentEditPageComponent } from './user-department-edit-page.component';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { JustContatorePipe } from '../../../shared/pipe/GestionePresenze/just-contatore.pipe';
import { ParProfiloOrarioToLongTextPipePipe } from '../../../shared/pipe/GestionePresenze/par-profilo-orario-to-long-text-pipe.pipe';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonButton, IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonIcon, IonInput, IonItem, IonItemSliding, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonSpinner, IonToggle,
    RouterModule.forChild([
      {
        path: '',
        component: UserDepartmentEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
    JustContatorePipe,
    ParProfiloOrarioToLongTextPipePipe,
  ],
  declarations: [UserDepartmentEditPageComponent],
})
export class UserDepartmentEditPageModule {}
