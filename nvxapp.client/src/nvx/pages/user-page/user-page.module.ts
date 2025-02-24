import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserPageComponent } from './user-page.component';
import { SharedComponentModule } from '../../shared/shared-component.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserPageComponent
      }
    ]),
    SharedComponentModule
  ],
  declarations: [UserPageComponent]
})
export class UserPageModule { }


