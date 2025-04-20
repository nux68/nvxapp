import { NgModule, LOCALE_ID, importProvidersFrom } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouteReuseStrategy } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { IonicModule, IonicRouteStrategy } from '@ionic/angular';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NvxHttpInterceptor } from '../nvx/http/http-interceptor';
import { HttpBackgroundWorkingComponentComponent } from '../nvx/shared/components/infrastructure/http-background-working-component/http-background-working-component.component';
import { BackgroundWorkingComponent } from '../nvx/shared/components/infrastructure/background-working/background-working.component';
import { FabMenuComponent } from '../nvx/shared/components/infrastructure/fab-menu/fab-menu.component';
import { FormsModule } from '@angular/forms';
import { HttpBackgroundErrorDialogComponent } from '../nvx/shared/components/infrastructure/http-background-error-dialog/http-background-error-dialog.component';
import { ParameterLoaderComponent } from '../nvx/shared/components/infrastructure/parameter-loader/parameter-loader.component';
//import { GenericFilterPipe } from '../nvx/pipe/generic-filter.pipe';
import localeIt from '@angular/common/locales/it';
import { registerLocaleData } from '@angular/common';

// Registra i dati di localizzazione italiana
registerLocaleData(localeIt); // Questa riga è essenziale!

@NgModule({
  declarations: [AppComponent,
    HttpBackgroundWorkingComponentComponent,
    HttpBackgroundErrorDialogComponent,
    ParameterLoaderComponent,
    BackgroundWorkingComponent,
    FabMenuComponent
  ],
  imports: [
            BrowserModule,
            IonicModule.forRoot(),
            AppRoutingModule,
            FormsModule
  ],
  providers: [
              { provide: LOCALE_ID, useValue: 'it' },
              provideHttpClient(withInterceptorsFromDi()),
              { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
              { provide: HTTP_INTERCEPTORS, useClass: NvxHttpInterceptor, multi: true },
              //importProvidersFrom(GenericFilterPipe)
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
