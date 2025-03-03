import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';


@Injectable({
  providedIn: 'root'
})
export class SignalrService {

  private _isConnect$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public get IsConnect$(): Observable<boolean> {
    return this._isConnect$.asObservable();
  }


  private hubConnection!: signalR.HubConnection;
  private eventSubjects: Map<string, Subject<any>> = new Map();

  constructor(private authService: AuthService) {
    //this.startConnection();
  }

  public startConnection() {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return; // Connessione già attiva
    }
    

    var hubUrl = environment.remoteData.signalrUri + 'chathub';

    this.hubConnection = new signalR.HubConnectionBuilder()
      //.withUrl(hubUrl)
      ////.withUrl(hubUrl, {
      ////  accessTokenFactory: () => {
      ////    const token = this.authService.Token; // Ottieni il token più recente
      ////    console.log('Bearer Token:', token); // Aggiungi questo per verificare
      ////    return token || ''; // Ritorna il token o una stringa vuota se non presente
      ////  },
      ////})
      .withUrl(hubUrl, {
        accessTokenFactory: async () => {
          const token = this.authService.Token;
          console.log("📡 Token inviato a SignalR:", token);
          return token;
        }
      })
      .configureLogging(signalR.LogLevel.Trace) 
      .withAutomaticReconnect()
      .build();

    setTimeout(() => {

      

      this.hubConnection.start()
        .then(res => {
          this._isConnect$.next(true);
          console.log('? SignalR connesso');
        })
        .catch(err => console.error('? Errore connessione SignalR:', err));

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
        this.eventSubjects.get(eventName)?.next(data);
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
