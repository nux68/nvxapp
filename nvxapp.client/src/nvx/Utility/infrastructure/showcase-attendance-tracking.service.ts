import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase della gestione presenze: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo. Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAttendanceTrackingService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi attendance.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('attendance.showcase.' + key, params) ?? 'attendance.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'attendance.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'calendar-clear-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'finger-print-outline', title: t('home.punti.timbratura.title'), text: t('home.punti.timbratura.text') },
          { icon: 'checkmark-done-outline', title: t('home.punti.richieste.title'), text: t('home.punti.richieste.text') },
          { icon: 'calendar-number-outline', title: t('home.punti.cartellino.title'), text: t('home.punti.cartellino.text') },
          { icon: 'receipt-outline', title: t('home.punti.noteSpese.title'), text: t('home.punti.noteSpese.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'git-network-outline', title: t('home.passi.organizza.title'), text: t('home.passi.organizza.text') },
          { icon: 'cloud-upload-outline', title: t('home.passi.chiudi.title'), text: t('home.passi.chiudi.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('mic-outline', 'features.sezioni.vocale'),
          v('finger-print-outline', 'features.sezioni.timbrature'),
          v('document-text-outline', 'features.sezioni.giustificativi'),
          v('time-outline', 'features.sezioni.straordinari'),
          v('people-circle-outline', 'features.sezioni.reparti'),
          v('calendar-outline', 'features.sezioni.calendario'),
          v('pulse-outline', 'features.sezioni.presenti'),
          v('speedometer-outline', 'features.sezioni.profili'),
          v('checkbox-outline', 'features.sezioni.quadratura'),
          v('download-outline', 'features.sezioni.export'),
          v('receipt-outline', 'features.sezioni.noteSpese'),
          v('briefcase-outline', 'features.sezioni.commesse'),
          v('options-outline', 'features.sezioni.menu')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.AttendanceTracking],
        inclusi: [t('pricing.inclusi.timbraturaSmartphone'), t('pricing.inclusi.exportPaghe'), t('pricing.inclusi.causaliPronte'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
          { domanda: t('pricing.domande.paghe.domanda'), risposta: t('pricing.domande.paghe.risposta') },
          { domanda: t('pricing.domande.approvazione.domanda'), risposta: t('pricing.domande.approvazione.risposta') },
          { domanda: t('pricing.domande.consulente.domanda'), risposta: t('pricing.domande.consulente.risposta') }
        ]
      },
      about: {
        titolo: t('about.titolo'),
        sottotitolo: t('about.sottotitolo'),
        paragrafi: [
          t('about.paragrafo1'),
          t('about.paragrafo2', site)
        ],
        valori: [
          { icon: 'happy-outline', title: t('about.valori.semplice.title'), text: t('about.valori.semplice.text') },
          { icon: 'shield-checkmark-outline', title: t('about.valori.preciso.title'), text: t('about.valori.preciso.text') },
          { icon: 'chatbubbles-outline', title: t('about.valori.vicino.title'), text: t('about.valori.vicino.text') }
        ]
      },
      register: {
        titolo: t('register.titolo'),
        sottotitolo: t('register.sottotitolo'),
        vantaggi: [t('register.vantaggi.gratis'), t('register.vantaggi.attiva'), t('register.vantaggi.nessunaCarta')],
        condizioni: t('register.condizioni'),
        inviata: t('register.inviata'),
        confermaTitolo: t('register.confermaTitolo'),
        esito: t('register.esito')
      }
    };
  }
}
