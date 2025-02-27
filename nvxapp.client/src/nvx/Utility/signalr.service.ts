import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class SignalrService {

  private hubConnection!: signalR.HubConnection;
  private eventSubjects: Map<string, Subject<any>> = new Map();

  constructor() {
    this.startConnection();
  }

  private startConnection() {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return; // Connessione già attiva
    }
    

    var hubUrl = environment.remoteData.signalrUri + 'chathub';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .configureLogging(signalR.LogLevel.Trace) 
      .withAutomaticReconnect()
      .build();

    setTimeout(() => {

      this.hubConnection.start()
        .then(() => console.log('? SignalR connesso'))
        .catch(err => console.error('? Errore connessione SignalR:', err));

    }, 5000); // Ritardo di 5 secondi

    

    this.hubConnection.onclose(() => {
      console.log('? Connessione persa, tentando riconnessione...');
      setTimeout(() => this.startConnection(), 3000);
    });


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
    this.hubConnection.invoke(eventName, data)
      .catch(err => console.error(`? Errore invio evento ${eventName}:`, err));
  }

}
