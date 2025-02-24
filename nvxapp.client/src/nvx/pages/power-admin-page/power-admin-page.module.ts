import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { PowerAdminPageComponent } from './power-admin-page.component';
import { SharedComponentModule } from '../../shared/shared-component.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: PowerAdminPageComponent
      }
    ]),
    SharedComponentModule
  ],
  declarations: [PowerAdminPageComponent]
})
export class PowerAdminPageModule { }
