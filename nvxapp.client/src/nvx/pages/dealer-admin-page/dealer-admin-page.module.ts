import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { DealerAdminPageComponent } from './dealer-admin-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DealerAdminPageComponent
      }
    ])
  ],

  declarations: [DealerAdminPageComponent],
  
})
export class DealerAdminPageModule { }
