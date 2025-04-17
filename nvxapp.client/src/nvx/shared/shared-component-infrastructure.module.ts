import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/infrastructure/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/infrastructure/breadcrumbs-impersonate/breadcrumbs-impersonate.component';
import { PageToolbarComponent } from './components/infrastructure/page-toolbar/page-toolbar.component';
import { FabMenuComponent } from './components/infrastructure/fab-menu/fab-menu.component';
import { FormsModule } from '@angular/forms';
import { PageButtonbarComponent } from './components/infrastructure/page-buttonbar/page-buttonbar.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    GenericFilterPipe
  ],
  declarations: [BreadcrumbsImpersonateComponent, PageToolbarComponent, PageButtonbarComponent/*, FabMenuComponent*/],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe, PageToolbarComponent, PageButtonbarComponent /*, FabMenuComponent*/]
})
export class SharedComponentInfrastructureModule { }
