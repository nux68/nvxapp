import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonButton, IonButtons, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSelect, IonSelectOption, IonTextarea, IonToggle } from '@ionic/angular';
import { SharedComponentInfrastructureModule } from './shared-component-infrastructure.module';
import { CrmNotesComponent } from './components/Crm/crm-notes/crm-notes.component';
import { CrmAttachmentsComponent } from './components/Crm/crm-attachments/crm-attachments.component';
import { CrmContactDialogComponent } from './components/Crm/crm-contact-dialog/crm-contact-dialog.component';
import { CrmLeadConvertDialogComponent } from './components/Crm/crm-lead-convert-dialog/crm-lead-convert-dialog.component';
import { CrmOfferSendDialogComponent } from './components/Crm/crm-offer-send-dialog/crm-offer-send-dialog.component';

// componenti del CRM usati da piu' pagine (note, allegati, dialog)
@NgModule({
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    IonButton, IonButtons, IonCard, IonCardContent, IonCheckbox, IonCol, IonContent, IonGrid, IonIcon, IonInput, IonItem, IonLabel, IonList, IonNote, IonRow, IonSelect, IonSelectOption, IonTextarea, IonToggle,
    SharedComponentInfrastructureModule
  ],
  declarations: [CrmNotesComponent, CrmAttachmentsComponent, CrmContactDialogComponent, CrmLeadConvertDialogComponent, CrmOfferSendDialogComponent],
  exports: [CrmNotesComponent, CrmAttachmentsComponent, CrmContactDialogComponent, CrmLeadConvertDialogComponent, CrmOfferSendDialogComponent]
})
export class SharedComponentCrmModule { }
