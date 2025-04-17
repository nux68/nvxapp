import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { UserDealerEditPageComponent } from './user-dealer-edit-page.component';

//

@NgModule({
  imports: [
    
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserDealerEditPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],

  declarations: [UserDealerEditPageComponent],

})
export class UserDealerEditPageModule { }
