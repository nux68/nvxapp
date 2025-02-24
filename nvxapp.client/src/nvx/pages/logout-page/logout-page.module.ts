import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { LogoutPageComponent } from './logout-page.component';
import { SharedComponentModule } from '../../shared/shared-component.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: LogoutPageComponent
      }
    ]),
    SharedComponentModule
  ],
  declarations: [LogoutPageComponent]
})
export class LogoutPageModule { }
