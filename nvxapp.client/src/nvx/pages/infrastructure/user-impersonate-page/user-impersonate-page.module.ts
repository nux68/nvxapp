import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { UserImpersonatePageComponent } from './user-impersonate-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserImpersonatePageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [UserImpersonatePageComponent]
})
export class UserImpersonatePageModule { }



