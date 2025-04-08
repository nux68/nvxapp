
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../../shared/shared-component.module';
import { BasePageConfirmCancelComponent } from './base-page-confirm-cancel.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    //RouterModule.forChild([
    //  {
    //    path: '',
    //    component: BasePageConfirmCancelComponent
    //  }
    //]),
    SharedComponentModule
  ],

  declarations: [/*BasePageConfirmCancelComponent*/ ],
  exports: [/*BasePageConfirmCancelComponent*/],

})
export class BasePageConfirmCancelModule { }


