import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonBreadcrumb, IonBreadcrumbs, IonButton, IonButtons, IonCard, IonCardContent, IonCol, IonContent, IonFooter, IonGrid, IonHeader, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonLabel, IonMenuButton, IonNote, IonProgressBar, IonRow, IonSearchbar, IonSelect, IonSelectOption, IonTextarea, IonTitle, IonToggle, IonToolbar } from '@ionic/angular';
import { GenericFilterPipe } from './pipe/infrastructure/generic-filter.pipe';
import { BreadcrumbsImpersonateComponent } from './components/infrastructure/breadcrumbs-impersonate/breadcrumbs-impersonate.component';
import { PageToolbarComponent } from './components/infrastructure/page-toolbar/page-toolbar.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { NvxDatetimeLocaleDirective } from './components/infrastructure/datetime-locale/datetime-locale.directive';
import { PageButtonbarComponent } from './components/infrastructure/page-buttonbar/page-buttonbar.component';
import { GenericDialogComponent } from './components/infrastructure/generic-dialog/generic-dialog.component';
import { HoverPopupComponent } from './components/infrastructure/hover-popup/hover-popup.component';
import { HoverPopupDirective } from './components/infrastructure/hover-popup/hover-popup.directive';
import { SlideButtonCancellaModificaComponent } from './components/infrastructure/slide-button-cancella-modifica/slide-button-cancella-modifica.component';
import { AddUserCompanyComponent } from './components/infrastructure/add-user-company/add-user-company.component';
import { AddCompanyComponent } from './components/infrastructure/add-company/add-company.component';
import { NvxListCellDirective, NvxListComponent, NvxListMobileDirective } from './components/infrastructure/nvx-list/nvx-list.component';
import { TicketAttachComponent } from './components/infrastructure/ticket-attach/ticket-attach.component';
import { TicketActionDialogComponent } from './components/infrastructure/ticket-action-dialog/ticket-action-dialog.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonBreadcrumb, IonBreadcrumbs, IonButton, IonButtons, IonCard, IonCardContent, IonCol, IonContent, IonFooter, IonGrid, IonHeader, IonIcon, IonInput, IonItem, IonItemOption, IonItemOptions, IonLabel, IonMenuButton, IonNote, IonProgressBar, IonRow, IonSearchbar, IonSelect, IonSelectOption, IonTextarea, IonTitle, IonToggle, IonToolbar,
    GenericFilterPipe,
    HoverPopupComponent, HoverPopupDirective,
    NvxListComponent, NvxListCellDirective, NvxListMobileDirective,
    TranslocoPipe,
    NvxDatetimeLocaleDirective
  ],
  declarations: [BreadcrumbsImpersonateComponent, PageToolbarComponent, SlideButtonCancellaModificaComponent, PageButtonbarComponent, GenericDialogComponent, AddUserCompanyComponent, AddCompanyComponent, TicketAttachComponent, TicketActionDialogComponent/*, FabMenuComponent*/],
  exports: [BreadcrumbsImpersonateComponent, GenericFilterPipe, PageToolbarComponent, SlideButtonCancellaModificaComponent, PageButtonbarComponent, HoverPopupComponent, HoverPopupDirective, AddUserCompanyComponent, AddCompanyComponent, TicketAttachComponent, TicketActionDialogComponent, NvxListComponent, NvxListCellDirective, NvxListMobileDirective, TranslocoPipe, NvxDatetimeLocaleDirective/*, FabMenuComponent*/]
})
export class SharedComponentInfrastructureModule { }
