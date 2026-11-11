import { Injectable } from '@angular/core';
import { formatDate, formatNumber } from '@angular/common';
import { LanguageService } from '../infrastructure/language.service';

/*
 Formati di Risorse aziendali nella lingua attiva (date, date/ore, importi) e valori per gli <ion-input> di tipo date/datetime-local.
 Le date del server sono locali, senza fuso.
*/
@Injectable({
  providedIn: 'root'
})
export class AstFormatService {

  constructor(private languageService: LanguageService) { }

  data(v: string | null | undefined): string {
    return v ? formatDate(v, 'shortDate', this.languageService.locale) : '';
  }

  dataOra(v: string | null | undefined): string {
    return v ? formatDate(v, 'short', this.languageService.locale) : '';
  }

  ora(v: string | null | undefined): string {
    return v ? formatDate(v, 'shortTime', this.languageService.locale) : '';
  }

  importo(v: number | null | undefined): string {
    return formatNumber(v ?? 0, this.languageService.locale, '1.2-2') + ' €';
  }
}

// valore per <ion-input type="date"> e "datetime-local"
export function astPerInputData(v: string | null | undefined): string | null {
  return v ? v.substring(0, 10) : null;
}

export function astPerInputDataOra(v: string | null | undefined): string | null {
  return v ? v.substring(0, 16) : null;
}

// data (e ora) locale nel formato del server, senza fuso
export function astOggi(giorni = 0): string {
  const d = new Date(Date.now() + giorni * 86400000);
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`;
}

export function astDataOraLocale(d: Date): string {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
}

// numero da un campo del form (stringa vuota = null)
export function astNumero(v: any): number | null {
  if (v === null || v === undefined || v === '') return null;
  const n = Number(v);
  return isNaN(n) ? null : n;
}
