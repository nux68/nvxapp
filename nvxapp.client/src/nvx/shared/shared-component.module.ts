import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/breadcrumbs-impersonate/breadcrumbs-impersonate.component';
import { PageToolbarComponent } from './components/page-toolbar/page-toolbar.component';





@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    GenericFilterPipe
  ],
  declarations: [BreadcrumbsImpersonateComponent, PageToolbarComponent],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe, PageToolbarComponent]
})
export class SharedComponentModule { }
