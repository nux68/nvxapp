import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SuperUserPageComponent } from './super-user-page.component';




@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: SuperUserPageComponent
      }
    ])
  ],
  declarations: [SuperUserPageComponent]
})
export class SuperUserPageModule { }
