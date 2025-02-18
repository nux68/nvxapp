import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { UserImpersonateComponent } from './user-impersonate.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: UserImpersonateComponent
      }
    ])
  ],
  declarations: [UserImpersonateComponent]
})
export class UserImpersonateModule { }



