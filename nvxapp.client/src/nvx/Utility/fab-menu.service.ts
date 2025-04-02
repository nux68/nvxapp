import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

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


  constructor() { }
}


export class FabMenuItem {

  constructor(
    public text: string | null = null,
    public image: string | null = null,
    public event: (() => void) | null = null
  ) { }

}
