import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase del CRM: stesse pagine template di Infrastructure (ShowcaseService), con i testi sulle funzioni del ramo.
// Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseCrmService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi crm.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('crm.showcase.' + key, params) ?? 'crm.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'crm.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'briefcase-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'business-outline', title: t('home.punti.clienti.title'), text: t('home.punti.clienti.text') },
          { icon: 'git-network-outline', title: t('home.punti.pipeline.title'), text: t('home.punti.pipeline.text') },
          { icon: 'calendar-outline', title: t('home.punti.agenda.title'), text: t('home.punti.agenda.text') },
          { icon: 'document-text-outline', title: t('home.punti.offerte.title'), text: t('home.punti.offerte.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'options-outline', title: t('home.passi.configura.title'), text: t('home.passi.configura.text') },
          { icon: 'trending-up-outline', title: t('home.passi.vendi.title'), text: t('home.passi.vendi.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('business-outline', 'features.sezioni.clienti'),
          v('person-add-outline', 'features.sezioni.lead'),
          v('git-network-outline', 'features.sezioni.pipeline'),
          v('calendar-outline', 'features.sezioni.agenda'),
          v('notifications-outline', 'features.sezioni.promemoria'),
          v('document-text-outline', 'features.sezioni.offerte'),
          v('attach-outline', 'features.sezioni.note'),
          v('eye-off-outline', 'features.sezioni.visibilita'),
          v('bar-chart-outline', 'features.sezioni.report'),
          v('swap-horizontal-outline', 'features.sezioni.riassegna'),
          v('download-outline', 'features.sezioni.export'),
          v('shield-checkmark-outline', 'features.sezioni.gdpr'),
          v('apps-outline', 'features.sezioni.integrato')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.Crm],
        inclusi: [t('pricing.inclusi.offertePdf'), t('pricing.inclusi.report'), t('pricing.inclusi.tabellePronte'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
          { domanda: t('pricing.domande.visibilita.domanda'), risposta: t('pricing.domande.visibilita.risposta') },
          { domanda: t('pricing.domande.presenze.domanda'), risposta: t('pricing.domande.presenze.risposta') },
          { domanda: t('pricing.domande.dati.domanda'), risposta: t('pricing.domande.dati.risposta') }
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
          { icon: 'shield-checkmark-outline', title: t('about.valori.ordinato.title'), text: t('about.valori.ordinato.text') },
          { icon: 'chatbubbles-outline', title: t('about.valori.vicino.title'), text: t('about.valori.vicino.text') }
        ]
      },
      register: {
        titolo: t('register.titolo'),
        sottotitolo: t('register.sottotitolo'),
        vantaggi: [t('register.vantaggi.gratis'), t('register.vantaggi.pronto'), t('register.vantaggi.nessunaCarta')],
        condizioni: t('register.condizioni'),
        inviata: t('register.inviata'),
        confermaTitolo: t('register.confermaTitolo'),
        esito: t('register.esito')
      }
    };
  }
}
