import { Injectable } from '@angular/core';
//import {
//  HttpInterceptor,
//  HttpRequest,
//  HttpHandler,
//  HttpEvent,
//  HttpErrorResponse,
//} from '@angular/common/http';

import { HttpClient, HttpInterceptor, HttpHandler, HttpEvent, HttpRequest } from '@angular/common/http';

import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
//import { AuthService } from '../auth/auth.service';
//import { CurrentSelectionService } from '../local-data/current-selection.service';
import { environment } from '../../environments/environment';


@Injectable()
export class NvxHttpInterceptor implements HttpInterceptor {

  constructor(
              //public currentSelectionService: CurrentSelectionService,
              private nvxHttpInterceptorService: NvxHttpInterceptorService
             )
  {
  }

  //intercept(
  //  request: HttpRequest<any>,
  //  next: HttpHandler
  //): Observable<HttpEvent<any>> {
  //  //console.log('Inizio della chiamata HTTP:', request.url);

  //  const apiUri = environment.remoteData.apiUri;

  //  if (request.url.includes( apiUri )) {

  //    //if (this.currentSelectionService.relationshipCronology && this.currentSelectionService.relationshipCronology.length>0) {
  //      request = request.clone({ headers: request.headers.set('UserName',  "SookaUser" /*this.currentSelectionService.relationshipCronology[0].userName*/) });
  //    //}

  //  }

  //  this.nvxHttpInterceptorService.HttpCal_Start(request.url);


  //  // Aggiungi il tuo codice di manipolazione della richiesta qui, se necessario

  //  return next.handle(request).pipe(
  //    finalize(() => {
  //      //console.log('Fine della chiamata HTTP');

  //      this.nvxHttpInterceptorService.HttpCal_End(request.url);

  //      // Aggiungi il tuo codice di manipolazione della risposta qui, se necessario
  //    })
  //  );
  //}



  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Clona la richiesta per aggiungere il nuovo header
    const authReq = req.clone({
      setHeaders: {
        UserName: `NVX` //SookaUser
      }
    });

    // Passa la richiesta al prossimo handler nella catena
    return next.handle(authReq);
  }

}


@Injectable({
  providedIn: 'root'
})
export class NvxHttpInterceptorService {

  private _activeCall: string[] = [];

  private _isActiveCall: boolean  = false;
  private _isActiveCall$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor() { }


  public get isActiveCall(): boolean {
    return this._isActiveCall ;
  }

  public get isActiveCall$(): Observable<boolean> {
    return this._isActiveCall$.asObservable();
  }


  HttpCal_Start(requestUrl:string): void {
    this._activeCall.push('');

    this._isActiveCall = true;
    this._isActiveCall$.next(this._isActiveCall);

  }

  HttpCal_End(requestUrl: string): void {
    this._activeCall.pop();

    if (this._activeCall.length == 0) {
      this._isActiveCall = false;
      this._isActiveCall$.next(this._isActiveCall);
    }

  }

}
