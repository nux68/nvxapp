import { Injectable } from '@angular/core';

/** Lingue gestite dall'applicazione; la prima e' quella di riferimento (testi originali). */
export const SUPPORTED_LANGUAGES = ['it', 'en'] as const;
export type Language = typeof SUPPORTED_LANGUAGES[number];
export const DEFAULT_LANGUAGE: Language = 'it';

// per ogni lingua: locale di Angular (date/numeri), lingua del riconoscimento vocale ed etichetta
const LANGUAGE_INFO: Record<Language, { locale: string; speech: string; label: string }> = {
  it: { locale: 'it', speech: 'it-IT', label: 'IT' },
  en: { locale: 'en-GB', speech: 'en-GB', label: 'EN' },
};

/*
 Lingua dell'interfaccia.
 La scelta dell'utente e' salvata nel browser (localStorage); finche' l'utente non sceglie si usa la
 lingua del browser, se gestita, altrimenti l'italiano.
 Il cambio di lingua ricarica la pagina: cosi' si aggiornano anche i testi calcolati nel codice
 (bottoni, menu, messaggi) e il LOCALE_ID di Angular, che si legge solo all'avvio.
 La lingua viene mandata al server in ogni chiamata (header Accept-Language, vedi http-interceptor).
*/
@Injectable({
  providedIn: 'root'
})
export class LanguageService {

  private static readonly StorageKey = 'nvx-lang';

  public readonly current: Language;

  constructor() {
    this.current = this.readSaved() ?? this.browserLanguage() ?? DEFAULT_LANGUAGE;
    document.documentElement.lang = this.current;
  }

  /** locale di Angular per date e numeri (LOCALE_ID) */
  public get locale(): string {
    return LANGUAGE_INFO[this.current].locale;
  }

  /** lingua per il riconoscimento vocale (Web Speech API) */
  public get speechLanguage(): string {
    return LANGUAGE_INFO[this.current].speech;
  }

  public get languages(): readonly Language[] {
    return SUPPORTED_LANGUAGES;
  }

  public label(language: Language): string {
    return LANGUAGE_INFO[language].label;
  }

  /** lingua successiva nell'elenco: e' quella proposta dal pulsante di cambio lingua */
  public get next(): Language {
    const index = SUPPORTED_LANGUAGES.indexOf(this.current);
    return SUPPORTED_LANGUAGES[(index + 1) % SUPPORTED_LANGUAGES.length];
  }

  public toggle(): void {
    this.setLanguage(this.next);
  }

  public setLanguage(language: Language): void {
    if (language === this.current)
      return;
    try { localStorage.setItem(LanguageService.StorageKey, language); } catch { }
    location.reload();
  }

  private readSaved(): Language | null {
    try {
      return this.asLanguage(localStorage.getItem(LanguageService.StorageKey));
    } catch {
      return null;
    }
  }

  private browserLanguage(): Language | null {
    for (const value of navigator.languages ?? [navigator.language]) {
      const language = this.asLanguage(value?.substring(0, 2).toLowerCase());
      if (language)
        return language;
    }
    return null;
  }

  private asLanguage(value: string | null | undefined): Language | null {
    return SUPPORTED_LANGUAGES.find(x => x === value) ?? null;
  }
}
