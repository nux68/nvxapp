import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

// Tema chiaro/scuro dell'applicazione.
// Il tema scuro si attiva con la classe 'ion-palette-dark' su <html> (palette Ionic dark.class.css,
// importata in global.scss); i colori dell'applicazione per il tema scuro sono in theme/variables.scss.
// La scelta dell'utente e' salvata nel browser; finche' l'utente non sceglie, si segue il sistema operativo.
@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private static readonly StorageKey = 'nvx-theme';
  private static readonly DarkClass = 'ion-palette-dark';

  private readonly systemDark = window.matchMedia('(prefers-color-scheme: dark)');
  private _isDark$: BehaviorSubject<boolean>;

  public get isDark$(): Observable<boolean> {
    return this._isDark$.asObservable();
  }

  public get isDark(): boolean {
    return this._isDark$.value;
  }

  constructor() {
    const saved = this.readSaved();
    this._isDark$ = new BehaviorSubject<boolean>(saved !== null ? saved === 'dark' : this.systemDark.matches);
    this.apply(this.isDark);

    // nessuna scelta salvata: il tema segue i cambi del sistema operativo
    this.systemDark.addEventListener('change', e => {
      if (this.readSaved() === null)
        this.setDark(e.matches, false);
    });
  }

  public toggle(): void {
    this.setDark(!this.isDark, true);
  }

  private setDark(dark: boolean, save: boolean): void {
    if (save) {
      try { localStorage.setItem(ThemeService.StorageKey, dark ? 'dark' : 'light'); } catch { }
    }
    this.apply(dark);
    this._isDark$.next(dark);
  }

  private apply(dark: boolean): void {
    document.documentElement.classList.toggle(ThemeService.DarkClass, dark);
  }

  private readSaved(): string | null {
    try {
      const value = localStorage.getItem(ThemeService.StorageKey);
      return value === 'dark' || value === 'light' ? value : null;
    } catch {
      return null;
    }
  }
}
