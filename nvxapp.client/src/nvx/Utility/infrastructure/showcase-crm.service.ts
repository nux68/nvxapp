import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';
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
          { icon: 'business-outline', title: t('features.sezioni.clienti.title'), text: t('features.sezioni.clienti.text') },
          { icon: 'person-add-outline', title: t('features.sezioni.lead.title'), text: t('features.sezioni.lead.text') },
          { icon: 'git-network-outline', title: t('features.sezioni.pipeline.title'), text: t('features.sezioni.pipeline.text') },
          { icon: 'calendar-outline', title: t('features.sezioni.agenda.title'), text: t('features.sezioni.agenda.text') },
          { icon: 'notifications-outline', title: t('features.sezioni.promemoria.title'), text: t('features.sezioni.promemoria.text') },
          { icon: 'document-text-outline', title: t('features.sezioni.offerte.title'), text: t('features.sezioni.offerte.text') },
          { icon: 'attach-outline', title: t('features.sezioni.note.title'), text: t('features.sezioni.note.text') },
          { icon: 'eye-off-outline', title: t('features.sezioni.visibilita.title'), text: t('features.sezioni.visibilita.text') },
          { icon: 'bar-chart-outline', title: t('features.sezioni.report.title'), text: t('features.sezioni.report.text') },
          { icon: 'swap-horizontal-outline', title: t('features.sezioni.riassegna.title'), text: t('features.sezioni.riassegna.text') },
          { icon: 'download-outline', title: t('features.sezioni.export.title'), text: t('features.sezioni.export.text') },
          { icon: 'shield-checkmark-outline', title: t('features.sezioni.gdpr.title'), text: t('features.sezioni.gdpr.text') },
          { icon: 'apps-outline', title: t('features.sezioni.integrato.title'), text: t('features.sezioni.integrato.text') }
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
