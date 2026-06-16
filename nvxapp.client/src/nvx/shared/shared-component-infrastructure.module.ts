import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/infrastructure/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/infrastructure/breadcrumbs-impersonate/breadcrumbs-impersonate.component';
import { PageToolbarComponent } from './components/infrastructure/page-toolbar/page-toolbar.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageButtonbarComponent } from './components/infrastructure/page-buttonbar/page-buttonbar.component';
import { GenericDialogComponent } from './components/infrastructure/generic-dialog/generic-dialog.component';
import { HoverPopupComponent } from './components/infrastructure/hover-popup/hover-popup.component';
import { HoverPopupDirective } from './components/infrastructure/hover-popup/hover-popup.directive';
import { SlideButtonCancellaModificaComponent } from './components/infrastructure/slide-button-cancella-modifica/slide-button-cancella-modifica.component';
import { AddUserCompanyComponent } from './components/infrastructure/add-user-company/add-user-company.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    GenericFilterPipe,
    HoverPopupComponent, HoverPopupDirective
  ],
  declarations: [BreadcrumbsImpersonateComponent, PageToolbarComponent, SlideButtonCancellaModificaComponent, PageButtonbarComponent, GenericDialogComponent, AddUserCompanyComponent/*, FabMenuComponent*/],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe, PageToolbarComponent, SlideButtonCancellaModificaComponent, PageButtonbarComponent, HoverPopupComponent, HoverPopupDirective, AddUserCompanyComponent/*, FabMenuComponent*/]
})
export class SharedComponentInfrastructureModule { }
