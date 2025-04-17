import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
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
    SharedComponentInfrastructureModule
  ],

  declarations: [UserDealerListPageComponent],

})
export class UserDealerListPageModule { }
