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

  // utente del token con cui e' aperta la connessione (claim "sub"): le notifiche mirate (Clients.User) vanno a lui
  private connectedUserId: string | null = null;

  constructor(private authService: AuthService,
              private ngZone: NgZone) {
    this.authService.Token$.subscribe(token => this.onTokenChanged(token));
  }

  public startConnection() {
    if (this.hubConnection && (this.hubConnection.state === signalR.HubConnectionState.Connected ||
                               this.hubConnection.state === signalR.HubConnectionState.Connecting ||
                               this.hubConnection.state === signalR.HubConnectionState.Reconnecting)) {
      return; // Connessione già attiva (o in corso): non se ne apre una seconda
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

    // ogni connessione e' un oggetto nuovo (dopo disconnessione, nuovo login o aggiornamento del token):
    // gli eventi gia' sottoscritti con on() vanno riattaccati, altrimenti le notifiche non arrivano piu'
    this.eventSubjects.forEach((_, eventName) => this.attachHandler(eventName));

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
      this.attachHandler(eventName);
    }
    return this.eventSubjects.get(eventName)!.asObservable();
  }

  // collega l'evento alla connessione corrente (una sola volta per connessione)
  private attachHandler(eventName: string) {
    if (!this.hubConnection) return; // si collega quando la connessione viene creata
    this.hubConnection.off(eventName);
    this.hubConnection.on(eventName, (data: any) => {
      // Esegui il callback dentro NgZone: SignalR gira fuori dalla zona Angular
      // e senza questo ogni messaggio causa una change-detection spuria che può
      // far ripartire la pagina corrente dall'inizio.
      this.ngZone.run(() => {
        this.eventSubjects.get(eventName)?.next(data);
      });
    });
  }

  // ?? Metodo per inviare un messaggio/evento generico
  send(eventName: string, data: any) {

    const token = this.authService.Token

    this.hubConnection.invoke(eventName, token,data)
      .catch(err => console.error(`? Errore invio evento ${eventName}:`, err));
  }

  /*
   Il token cambia a ogni risposta del server, ma cambia utente solo con l'impersonificazione (o tornando indietro).
   L'hub conosce l'utente dal token usato all'apertura della connessione: se l'utente cambia si riapre la connessione,
   altrimenti le notifiche mirate all'utente impersonato (ticket, job) arriverebbero ancora a quello del login.
  */
  private onTokenChanged(token: string | null): void {
    const userId = this.userIdOf(token);
    // al logout il token si svuota: si tiene l'ultimo utente, cosi' un nuovo login con un altro utente riapre la connessione
    if (userId == null)
      return;

    const cambiato = this.connectedUserId != null && userId !== this.connectedUserId;
    this.connectedUserId = userId;

    if (cambiato && this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected)
      this.restartConnection();
  }

  // id dell'utente (claim "sub") letto dal payload del JWT, senza verificarlo: serve solo a capire se e' cambiato
  private userIdOf(token: string | null): string | null {
    try {
      const payload = token?.split('.')[1];
      if (!payload) return null;
      const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(json).sub ?? null;
    }
    catch {
      return null;
    }
  }

}
