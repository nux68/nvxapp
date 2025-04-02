import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../shared/shared-component.module';
import { DealerEditPageComponent } from './dealer-edit-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DealerEditPageComponent
      }
    ]),
    SharedComponentModule
  ],

  declarations: [DealerEditPageComponent],

})
export class DealerEditPageModule { }



