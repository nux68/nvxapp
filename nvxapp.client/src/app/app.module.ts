import { NgModule, LOCALE_ID } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouteReuseStrategy } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { IonicModule, IonicRouteStrategy } from '@ionic/angular';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NvxHttpInterceptor } from '../nvx/infrastructure/http-interceptor ';


@NgModule({
  declarations: [AppComponent],
  imports: [
            BrowserModule,
            IonicModule.forRoot(),
            AppRoutingModule
  ],
  providers: [
              { provide: LOCALE_ID, useValue: 'it' },
              provideHttpClient(),
              { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
              { provide: HTTP_INTERCEPTORS,  useClass: NvxHttpInterceptor, multi: true }
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
