import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CompanyAdminPageComponent } from './company-admin-page.component';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyAdminPageComponent
      }
    ])
  ],
  declarations: [CompanyAdminPageComponent]
})
export class CompanyAdminPageModule { }
