import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/breadcrumbs-impersonate/breadcrumbs-impersonate.component';
import { PageToolbarComponent } from './components/page-toolbar/page-toolbar.component';
import { FabMenuComponent } from './components/fab-menu/fab-menu.component';
import { FormsModule } from '@angular/forms';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    GenericFilterPipe
  ],
  declarations: [BreadcrumbsImpersonateComponent, PageToolbarComponent /*, FabMenuComponent*/],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe, PageToolbarComponent /*, FabMenuComponent*/]
})
export class SharedComponentModule { }
