import { NgModule, LOCALE_ID, importProvidersFrom } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouteReuseStrategy } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { IonicModule, IonicRouteStrategy } from '@ionic/angular';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NvxHttpInterceptor } from '../nvx/infrastructure/http-interceptor';
import { HttpBackgroundWorkingComponentComponent } from '../nvx/shared/components/http-background-working-component/http-background-working-component.component';
import { BackgroundWorkingComponent } from '../nvx/shared/components/background-working/background-working.component';
import { GenericFilterPipe } from '../nvx/pipe/generic-filter.pipe';


@NgModule({
  declarations: [AppComponent,
    HttpBackgroundWorkingComponentComponent,
    BackgroundWorkingComponent],
  imports: [
            BrowserModule,
            IonicModule.forRoot(),
            AppRoutingModule
  ],
  providers: [
              { provide: LOCALE_ID, useValue: 'it' },
              provideHttpClient(withInterceptorsFromDi()),
              { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
              { provide: HTTP_INTERCEPTORS, useClass: NvxHttpInterceptor, multi: true },
              importProvidersFrom(GenericFilterPipe)
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
