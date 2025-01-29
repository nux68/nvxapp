import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { DomainAdminPageComponent } from './domain-admin-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DomainAdminPageComponent
      }
    ])
  ],
  declarations: [DomainAdminPageComponent]
})
export class DomainAdminPageModule { }
