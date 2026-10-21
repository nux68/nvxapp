import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FabMenuService {

  private _fabMenuItem: FabMenuItem[] | null = [];
  private _fabMenuItem$: BehaviorSubject<FabMenuItem[] | null> = new BehaviorSubject<FabMenuItem[]>(null);

  public get fabMenuItem$(): Observable<FabMenuItem[]> {
    return this._fabMenuItem$.asObservable();
  }

  public get fabMenuItem(): FabMenuItem[] | null {
    return this._fabMenuItem;
  }

  public set fabMenuItem(item:FabMenuItem[] | null) {
    this._fabMenuItem = item;
    this._fabMenuItem$.next(item);
  }

  // richiesta di apertura della chat dell'assistente da altre pagine (es. home); la gestisce FabMenuComponent
  private _openChat$ = new Subject<void>();

  public get openChat$(): Observable<void> {
    return this._openChat$.asObservable();
  }

  public openChat(): void {
    this._openChat$.next();
  }

  // comando dell'assistente (chat o voce) eseguito: le pagine con dati che il comando puo' aver cambiato
  // (es. i calendari delle presenze) si ricaricano subito, senza aspettare le notifiche SignalR
  private _commandExecuted$ = new Subject<void>();

  public get commandExecuted$(): Observable<void> {
    return this._commandExecuted$.asObservable();
  }

  public commandExecuted(): void {
    this._commandExecuted$.next();
  }


  constructor() { }
}


export class FabMenuItem {

  constructor(
    public text: string | null = null,
    public image: string | null = null,
    public event: () => void = () => { } // Arrow function di default
  ) { }

}
