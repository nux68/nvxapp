import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase di Risorse aziendali: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo. Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAssetsService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi ast.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('ast.showcase.' + key, params) ?? 'ast.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'ast.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'cube-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'car-outline', title: t('home.punti.flotta.title'), text: t('home.punti.flotta.text') },
          { icon: 'alarm-outline', title: t('home.punti.scadenze.title'), text: t('home.punti.scadenze.text') },
          { icon: 'laptop-outline', title: t('home.punti.beni.title'), text: t('home.punti.beni.text') },
          { icon: 'today-outline', title: t('home.punti.prenotazioni.title'), text: t('home.punti.prenotazioni.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'cube-outline', title: t('home.passi.censisci.title'), text: t('home.passi.censisci.text') },
          { icon: 'rocket-outline', title: t('home.passi.lavora.title'), text: t('home.passi.lavora.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('mic-outline', 'features.sezioni.vocale'),
          v('car-outline', 'features.sezioni.flotta'),
          v('alarm-outline', 'features.sezioni.scadenzeVeicoli'),
          v('cash-outline', 'features.sezioni.multe'),
          v('laptop-outline', 'features.sezioni.beni'),
          v('swap-horizontal-outline', 'features.sezioni.consegne'),
          v('today-outline', 'features.sezioni.prenotazioni'),
          v('person-circle-outline', 'features.sezioni.mieRisorse')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.Assets],
        inclusi: [t('pricing.inclusi.flotta'), t('pricing.inclusi.beni'), t('pricing.inclusi.prenotazioni'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
          { domanda: t('pricing.domande.scadenze.domanda'), risposta: t('pricing.domande.scadenze.risposta') },
          { domanda: t('pricing.domande.presenze.domanda'), risposta: t('pricing.domande.presenze.risposta') },
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
          { icon: 'file-tray-full-outline', title: t('about.valori.ordinato.title'), text: t('about.valori.ordinato.text') },
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
