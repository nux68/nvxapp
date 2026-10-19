import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { SharedComponentGestionePresenzeModuleModule } from '../../../shared/shared-component-gestione-presenze-module.module';
import { NotaSpeseUserPageComponent } from './nota-spese-user-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: NotaSpeseUserPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
    SharedComponentGestionePresenzeModuleModule,
  ],
  declarations: [NotaSpeseUserPageComponent]
})
export class NotaSpeseUserPageModule { }
