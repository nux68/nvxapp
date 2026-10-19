import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { NotaSpesaRequestUserPageComponent } from './nota-spesa-request-user-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: NotaSpesaRequestUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
  ],
  declarations: [NotaSpesaRequestUserPageComponent]
})
export class NotaSpesaRequestUserPageModule { }
