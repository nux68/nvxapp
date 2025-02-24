import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { CompanyAdminPageComponent } from './company-admin-page.component';
import { SharedComponentModule } from '../../shared/shared-component.module';



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
    ]),
    SharedComponentModule
  ],
  declarations: [CompanyAdminPageComponent]
})
export class CompanyAdminPageModule { }
