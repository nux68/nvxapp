// --- START OF FILE month-navigator.service.ts ---

import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MonthNavigatorService {
  private _currentMonth: number;
  private _currentYear: number;

  // --- NUOVO: Definizioni costanti per nomi giorni/mesi ---
  private readonly _weekDays = ['lun', 'mar', 'mer', 'gio', 'ven', 'sab', 'dom'];
  private readonly _monthNames = ['GENNAIO', 'FEBBRAIO', 'MARZO', 'APRILE', 'MAGGIO', 'GIUGNO',
    'LUGLIO', 'AGOSTO', 'SETTEMBRE', 'OTTOBRE', 'NOVEMBRE', 'DICEMBRE'];
  // --- FINE NUOVO ---

  constructor() {
    const today = new Date();
    this._currentMonth = today.getMonth();
    this._currentYear = today.getFullYear();
  }

  get currentMonth(): number {
    return this._currentMonth;
  }

  get currentYear(): number {
    return this._currentYear;
  }

  // MODIFICATO: Usa la costante interna
  get currentMonthName(): string {
    // Check per evitare errori se _currentMonth fosse invalido (improbabile qui)
    if (this._currentMonth >= 0 && this._currentMonth < this._monthNames.length) {
      return this._monthNames[this._currentMonth];
    }
    // Fallback se l'indice non è valido
    return new Date(this._currentYear, this._currentMonth, 1)
      .toLocaleString('it-IT', { month: 'long' }).toUpperCase(); // Usa localizzazione come fallback
  }

  // --- NUOVO: Getters per le costanti ---
  /**
   * Restituisce l'array dei nomi abbreviati dei giorni della settimana (lun-dom)
   */
  get weekDays(): string[] {
    return this._weekDays;
  }

  /**
   * Restituisce l'array dei nomi completi dei mesi (GENNAIO-DICEMBRE)
   */
  get monthNames(): string[] {
    return this._monthNames;
  }
  // --- FINE NUOVO ---

  nextMonth(): void {
    if (this._currentMonth === 11) {
      this._currentMonth = 0;
      this._currentYear++;
    } else {
      this._currentMonth++;
    }
  }

  previousMonth(): void {
    if (this._currentMonth === 0) {
      this._currentMonth = 11;
      this._currentYear--;
    } else {
      this._currentMonth--;
    }
  }

  setMonthAndYear(month: number, year: number): void {
    if (month < 0 || month > 11) {
      throw new Error('Il mese deve essere un valore tra 0 e 11');
    }
    this._currentMonth = month;
    this._currentYear = year;
  }

  resetToCurrentDate(): void {
    const today = new Date();
    this._currentMonth = today.getMonth();
    this._currentYear = today.getFullYear();
  }

  getFirstDayOfMonth(): Date {
    return new Date(this._currentYear, this._currentMonth, 1);
  }

  getLastDayOfMonth(): Date {
    return new Date(this._currentYear, this._currentMonth + 1, 0);
  }

  isCurrentMonthToday(): boolean {
    const today = new Date();
    return this._currentMonth === today.getMonth() &&
      this._currentYear === today.getFullYear();
  }
}
// --- END OF FILE month-navigator.service.ts ---
