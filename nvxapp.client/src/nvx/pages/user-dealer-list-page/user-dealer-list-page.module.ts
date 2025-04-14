import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentModule } from '../../shared/shared-component.module';
import { UserDealerListPageComponent } from './user-dealer-list-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserDealerListPageComponent
      }
    ]),
    SharedComponentModule
  ],

  declarations: [UserDealerListPageComponent],

})
export class UserDealerListPageModule { }
