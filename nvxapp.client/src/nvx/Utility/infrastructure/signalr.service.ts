import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, ReplaySubject, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';


@Injectable({
  providedIn: 'root'
})
export class SignalrService {

  // ReplaySubject(1): gli subscriber tardivi ricevono l'ultimo stato,
  // ma al contrario di BehaviorSubject non ri-emette ai subscriber già attivi
  // quando SignalR riconnette (evita handler duplicati in app.component.ts).
  private _isConnect$ = new ReplaySubject<boolean>(1);
  private _alreadySubscribed = false;
  public get IsConnect$(): Observable<boolean> {
    return this._isConnect$.asObservable();
  }


  private hubConnection!: signalR.HubConnection;
  private eventSubjects: Map<string, Subject<any>> = new Map();

  constructor(private authService: AuthService,
              private ngZone: NgZone) { }

  public startConnection() {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return; // Connessione già attiva
    }
    

    var hubUrl = environment.remoteData.signalrUri + 'chathub';

    this.hubConnection = new signalR.HubConnectionBuilder()
      //.withUrl(hubUrl)
      .withUrl(hubUrl, {
        accessTokenFactory: async () => {
          const token = this.authService.Token;
          console.log("Token inviato a SignalR:", token);
          return token;
        }
      })
      .configureLogging(signalR.LogLevel.Warning)  // Trace genera log per ogni frame WebSocket (inclusi ping) causando change-detection inattesi
      .withAutomaticReconnect()
      .build();

    setTimeout(() => {

      this.hubConnection.start()
        .then(res => {
          this.ngZone.run(() => {
            if (!this._alreadySubscribed) {
              this._alreadySubscribed = true;
              this._isConnect$.next(true);
            }
          });
          console.log('✅ SignalR connesso');
        })
        .catch(err => console.error('❌ Errore connessione SignalR:', err));

    }, 100); // Ritardo di 5 secondi

    

    this.hubConnection.onclose(() => {
      console.log('? Connessione persa, tentando riconnessione...');
      setTimeout(() => this.startConnection(), 3000);
    });


  }

  public restartConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => {
          console.log('⚠️ Connessione interrotta per aggiornare il token.');
          this.startConnection(); // Avvia nuovamente la connessione
        })
        .catch(err => console.error('❌ Errore durante la disconnessione:', err));
    }
  }

  // ?? Metodo per iscriversi a un evento generico
  on(eventName: string) {
    if (!this.eventSubjects.has(eventName)) {
      this.eventSubjects.set(eventName, new Subject<any>());
      this.hubConnection.on(eventName, (data: any) => {
        // Esegui il callback dentro NgZone: SignalR gira fuori dalla zona Angular
        // e senza questo ogni messaggio causa una change-detection spuria che può
        // far ripartire la pagina corrente dall'inizio.
        this.ngZone.run(() => {
          this.eventSubjects.get(eventName)?.next(data);
        });
      });
    }
    return this.eventSubjects.get(eventName)!.asObservable();
  }

  // ?? Metodo per inviare un messaggio/evento generico
  send(eventName: string, data: any) {

    const token = this.authService.Token

    this.hubConnection.invoke(eventName, token,data)
      .catch(err => console.error(`? Errore invio evento ${eventName}:`, err));
  }

}
