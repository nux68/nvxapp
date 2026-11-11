import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { CmpEditPageComponent } from './cmp-edit-page.component';

// stessa pagina per /cmpedit (azienda) e /cmpstudioedit (studio)
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonCard, IonCardContent, IonContent, IonInput, IonItem, IonLabel, IonNote, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    RouterModule.forChild([{ path: '', component: CmpEditPageComponent }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CmpEditPageComponent],
})
export class CmpEditPageModule { }
