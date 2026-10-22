import { EnvironmentProviders, Injectable, LOCALE_ID, Provider, inject, isDevMode, provideAppInitializer } from '@angular/core';
import { HttpBackend, HttpClient } from '@angular/common/http';
import { registerLocaleData } from '@angular/common';
import localeIt from '@angular/common/locales/it';
import localeEnGb from '@angular/common/locales/en-GB';
import { Translation, TranslocoLoader, TranslocoService, provideTransloco } from '@jsverse/transloco';
import { firstValueFrom, forkJoin, map, Observable, of, catchError } from 'rxjs';
import { DEFAULT_LANGUAGE, LanguageService, SUPPORTED_LANGUAGES } from './language.service';
import { I18N_FOLDERS } from './i18n-folders';

registerLocaleData(localeIt);
registerLocaleData(localeEnGb);

/*
 Carica le traduzioni di una lingua unendo i file di tutte le cartelle (I18N_FOLDERS).
 Per una lingua diversa dall'italiano si parte dai testi italiani e si sovrappongono quelli tradotti:
 una chiave non ancora tradotta mostra il testo italiano invece della chiave.
 Usa HttpBackend: le chiamate non passano dall'interceptor (niente token, niente spinner).
*/
@Injectable({ providedIn: 'root' })
export class NvxTranslocoLoader implements TranslocoLoader {

  private readonly http = new HttpClient(inject(HttpBackend));

  getTranslation(lang: string): Observable<Translation> {
    const languages = lang === DEFAULT_LANGUAGE ? [lang] : [DEFAULT_LANGUAGE, lang];
    const files = languages.flatMap(l => I18N_FOLDERS.map(folder => `assets/i18n/${folder}/${l}.json`));

    return forkJoin(files.map(file => this.http.get<Translation>(file).pipe(
      catchError(() => of({} as Translation))   // un ramo puo' non avere ancora il file di una lingua
    ))).pipe(
      map(translations => translations.reduce((all, t) => deepMerge(all, t), {} as Translation))
    );
  }
}

function deepMerge(target: Translation, source: Translation): Translation {
  const result: Translation = { ...target };
  for (const key of Object.keys(source)) {
    const value = source[key];
    result[key] = value && typeof value === 'object' && !Array.isArray(value) && typeof result[key] === 'object'
      ? deepMerge(result[key], value)
      : value;
  }
  return result;
}

/**
 * Multilingua (Transloco): da mettere tra i providers di app.module.
 * Le traduzioni della lingua scelta si caricano prima dell'avvio, quindi translate() si puo' usare
 * in modo sincrono anche nei servizi e nei costruttori.
 */
export function provideNvxI18n(): (Provider | EnvironmentProviders)[] {
  return [
    provideTransloco({
      config: {
        availableLangs: [...SUPPORTED_LANGUAGES],
        defaultLang: DEFAULT_LANGUAGE,
        reRenderOnLangChange: false,   // il cambio lingua ricarica la pagina (LanguageService)
        prodMode: !isDevMode(),
        // una chiave mancante mostra la chiave stessa: cosi' i testi non ancora convertiti
        // (es. titoli di menu scritti in chiaro) si vedono come sono
        missingHandler: { logMissingKey: false },
      },
      loader: NvxTranslocoLoader,
    }),
    { provide: LOCALE_ID, useFactory: () => inject(LanguageService).locale },
    provideAppInitializer(() => {
      const transloco = inject(TranslocoService);
      const language = inject(LanguageService).current;
      transloco.setActiveLang(language);
      return firstValueFrom(transloco.load(language));
    }),
  ];
}
