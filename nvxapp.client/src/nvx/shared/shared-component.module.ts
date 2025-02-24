import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/breadcrumbs-impersonate/breadcrumbs-impersonate.component';





@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    GenericFilterPipe
  ],
  declarations: [BreadcrumbsImpersonateComponent],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe]
})
export class SharedComponentModule { }
