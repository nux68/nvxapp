import { Injectable } from '@angular/core';
import { HttpClient, HttpInterceptor, HttpHandler, HttpEvent, HttpRequest, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthService } from '../Utility/auth.service';
import { NavController } from '@ionic/angular';
import { UserNavigationService } from '../Utility/user-navigation.service';
import { GenericResult } from '../ClientServer-Service/ModelsBase/generic-result';


@Injectable()
export class NvxHttpInterceptor implements HttpInterceptor {

  constructor(
              private authService: AuthService,
              private userNavigationService:UserNavigationService,
              private nvxHttpInterceptorService: NvxHttpInterceptorService,
              private navCtrl: NavController,
             )
  {
  }

  

  intercept(req: HttpRequest<any>,
            next: HttpHandler): Observable<HttpEvent<any>> {
    // Clona la richiesta per aggiungere il nuovo header
    const authReq = req.clone({
      setHeaders: {
        //UserName: this.authService.UserName != null ? this.authService.UserName : ``,  //`NVX` //SookaUser
        Authorization: `Bearer ${this.authService.Token}`
      }
    });

    this.nvxHttpInterceptorService.HttpCal_Start(req.url);

    // Passa la richiesta al prossimo handler nella catena
    return next.handle(authReq).pipe(
      tap((event: HttpEvent<any>) => {
        if (event instanceof HttpResponse) {
          const body = event.body;

          if (body && typeof body.success === 'boolean' && 'data' in body && Array.isArray(body.messages) && 'token' in body) {
            if (body.success === false) {
              this.nvxHttpInterceptorService.HttpError_Add(body);
            }
          }


          // Se la risposta contiene un nuovo token, aggiorna quello salvato
          const newToken = body?.token;
          if (newToken) {
            this.authService.Token = newToken;
          }
        }
      }),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          console.warn('Errore 401: Token scaduto o non valido.');

          this.userNavigationService.LogOut();  
          this.navCtrl.navigateForward('/home');
        }

        return throwError(() => error);
      }),
      finalize(() => {
        //console.log('Fine della chiamata HTTP');

        this.nvxHttpInterceptorService.HttpCal_End(req.url);

        // Aggiungi il tuo codice di manipolazione della risposta qui, se necessario
      })
      
    );
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



  public get isHttpErr(): boolean {
    return this._isHttpActiveErr;
  }

  public get isHttpErr$(): Observable<boolean> {
    return this._isActiveHttpErr$.asObservable();
  }

  public get activeHttpErr(): GenericResult<any>[] {
    return this._activeHttpErr;
  }


  private _activeHttpErr: GenericResult<any>[] = [];

  private _isHttpActiveErr: boolean = false;
  private _isActiveHttpErr$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  HttpError_Add(err: GenericResult<any>): void {
    this._activeHttpErr.push(err);

    this._isHttpActiveErr = true;
    this._isActiveHttpErr$.next(this._isHttpActiveErr);

  }

  HttpError_Clear(): void {
    this._activeHttpErr = [];

    this._isHttpActiveErr = false;
    this._isActiveHttpErr$.next(this._isHttpActiveErr);

  }

}
