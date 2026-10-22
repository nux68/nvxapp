// --- START OF FILE month-navigator.service.ts ---

import { Injectable } from '@angular/core';
import { LanguageService } from './language.service';

@Injectable({
  providedIn: 'root'
})
export class MonthNavigatorService {
  private _currentMonth: number;
  private _currentYear: number;

  // nomi di giorni e mesi nella lingua dell'interfaccia (es. 'lun'..'dom' / 'Mon'..'Sun', 'GENNAIO' / 'JANUARY')
  private readonly _weekDays: string[];
  private readonly _monthNames: string[];

  constructor(languageService: LanguageService) {
    const locale = languageService.locale;
    const weekDay = new Intl.DateTimeFormat(locale, { weekday: 'short' });
    const month = new Intl.DateTimeFormat(locale, { month: 'long' });
    // 2024-01-01 e' un lunedi': settimana da lunedi' a domenica
    this._weekDays = [...Array(7).keys()].map(i => weekDay.format(new Date(2024, 0, 1 + i)).replace('.', ''));
    this._monthNames = [...Array(12).keys()].map(i => month.format(new Date(2024, i, 1)).toUpperCase());

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
      .toLocaleString(undefined, { month: 'long' }).toUpperCase(); // Usa localizzazione come fallback
  }

  // --- NUOVO: Getters per le costanti ---
  /**
   * Restituisce l'array dei nomi abbreviati dei giorni della settimana (lun-dom), nella lingua dell'interfaccia
   */
  get weekDays(): string[] {
    return this._weekDays;
  }

  /**
   * Restituisce l'array dei nomi completi dei mesi (GENNAIO-DICEMBRE), nella lingua dell'interfaccia
   */
  get monthNames(): string[] {
    return this._monthNames;
  }
  // --- FINE NUOVO ---

  //nextMonth(): void {
  //  if (this._currentMonth === 11) {
  //    this._currentMonth = 0;
  //    this._currentYear++;
  //  } else {
  //    this._currentMonth++;
  //  }
  //}

  //previousMonth(): void {
  //  if (this._currentMonth === 0) {
  //    this._currentMonth = 11;
  //    this._currentYear--;
  //  } else {
  //    this._currentMonth--;
  //  }
  //}

  //setMonthAndYear(month: number, year: number): void {
  //  if (month < 0 || month > 11) {
  //    throw new Error('Il mese deve essere un valore tra 0 e 11');
  //  }
  //  this._currentMonth = month;
  //  this._currentYear = year;
  //}

  //resetToCurrentDate(): void {
  //  const today = new Date();
  //  this._currentMonth = today.getMonth();
  //  this._currentYear = today.getFullYear();
  //}

  //getFirstDayOfMonth(): Date {
  //  return new Date(this._currentYear, this._currentMonth, 1);
  //}

  //getLastDayOfMonth(): Date {
  //  return new Date(this._currentYear, this._currentMonth + 1, 0);
  //}

  //isCurrentMonthToday(): boolean {
  //  const today = new Date();
  //  return this._currentMonth === today.getMonth() &&
  //    this._currentYear === today.getFullYear();
  //}
}
// --- END OF FILE month-navigator.service.ts ---
