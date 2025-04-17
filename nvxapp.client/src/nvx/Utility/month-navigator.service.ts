import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MonthNavigatorService {
  private _currentMonth: number;
  private _currentYear: number;

  constructor() {
    // Inizializza con il mese e anno correnti
    const today = new Date();
    this._currentMonth = today.getMonth();
    this._currentYear = today.getFullYear();
  }

  /**
   * Ottiene il mese corrente (0-11, dove 0 è gennaio)
   */
  get currentMonth(): number {
    return this._currentMonth;
  }

  /**
   * Ottiene l'anno corrente
   */
  get currentYear(): number {
    return this._currentYear;
  }

  /**
   * Ottiene il nome del mese corrente
   */
  get currentMonthName(): string {
    return new Date(this._currentYear, this._currentMonth, 1)
      .toLocaleString('default', { month: 'long' });
  }

  /**
   * Vai al mese successivo
   */
  nextMonth(): void {
    if (this._currentMonth === 11) {
      this._currentMonth = 0;
      this._currentYear++;
    } else {
      this._currentMonth++;
    }
  }

  /**
   * Vai al mese precedente
   */
  previousMonth(): void {
    if (this._currentMonth === 0) {
      this._currentMonth = 11;
      this._currentYear--;
    } else {
      this._currentMonth--;
    }
  }

  /**
   * Imposta un mese e anno specifici
   * @param month Mese (0-11)
   * @param year Anno
   */
  setMonthAndYear(month: number, year: number): void {
    if (month < 0 || month > 11) {
      throw new Error('Il mese deve essere un valore tra 0 e 11');
    }
    this._currentMonth = month;
    this._currentYear = year;
  }

  /**
   * Resetta al mese e anno correnti
   */
  resetToCurrentDate(): void {
    const today = new Date();
    this._currentMonth = today.getMonth();
    this._currentYear = today.getFullYear();
  }

  /**
   * Ottiene la data del primo giorno del mese corrente
   */
  getFirstDayOfMonth(): Date {
    return new Date(this._currentYear, this._currentMonth, 1);
  }

  /**
   * Ottiene la data dell'ultimo giorno del mese corrente
   */
  getLastDayOfMonth(): Date {
    return new Date(this._currentYear, this._currentMonth + 1, 0);
  }

  /**
   * Controlla se il mese corrente è il mese attuale
   */
  isCurrentMonthToday(): boolean {
    const today = new Date();
    return this._currentMonth === today.getMonth() &&
      this._currentYear === today.getFullYear();
  }
}
