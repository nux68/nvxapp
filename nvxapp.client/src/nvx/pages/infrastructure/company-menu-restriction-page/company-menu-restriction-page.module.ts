import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CompanyMenuRestrictionPageComponent } from './company-menu-restriction-page.component';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';



@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: CompanyMenuRestrictionPageComponent
      }
    ]),
    SharedComponentInfrastructureModule
  ],
  declarations: [CompanyMenuRestrictionPageComponent]
})
export class CompanyMenuRestrictionPageModule { }
