import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase dell'applicativo completo (ramo AllFunction): presenze, CRM e documentale insieme, sulle stesse pagine template
// di Infrastructure. Ha la precedenza sulle vetrine dei singoli rami (vedi ShowcaseService). Testi: allfunction.showcase.*
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAllFunctionService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti: calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('allfunction.showcase.' + key, params) ?? 'allfunction.showcase.' + key;
    const site = { siteName: SITE_NAME };
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'apps-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'calendar-clear-outline', title: t('home.punti.presenze.title'), text: t('home.punti.presenze.text') },
          { icon: 'git-network-outline', title: t('home.punti.crm.title'), text: t('home.punti.crm.text') },
          { icon: 'documents-outline', title: t('home.punti.documentale.title'), text: t('home.punti.documentale.text') },
          { icon: 'link-outline', title: t('home.punti.insieme.title'), text: t('home.punti.insieme.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'toggle-outline', title: t('home.passi.attiva.title'), text: t('home.passi.attiva.text') },
          { icon: 'rocket-outline', title: t('home.passi.lavora.title'), text: t('home.passi.lavora.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          { icon: 'finger-print-outline', title: t('features.sezioni.timbrature.title'), text: t('features.sezioni.timbrature.text') },
          { icon: 'checkmark-done-outline', title: t('features.sezioni.richieste.title'), text: t('features.sezioni.richieste.text') },
          { icon: 'receipt-outline', title: t('features.sezioni.noteSpese.title'), text: t('features.sezioni.noteSpese.text') },
          { icon: 'download-outline', title: t('features.sezioni.paghe.title'), text: t('features.sezioni.paghe.text') },
          { icon: 'business-outline', title: t('features.sezioni.clienti.title'), text: t('features.sezioni.clienti.text') },
          { icon: 'git-network-outline', title: t('features.sezioni.pipeline.title'), text: t('features.sezioni.pipeline.text') },
          { icon: 'calendar-outline', title: t('features.sezioni.agenda.title'), text: t('features.sezioni.agenda.text') },
          { icon: 'document-text-outline', title: t('features.sezioni.offerte.title'), text: t('features.sezioni.offerte.text') },
          { icon: 'folder-open-outline', title: t('features.sezioni.archivio.title'), text: t('features.sezioni.archivio.text') },
          { icon: 'people-outline', title: t('features.sezioni.personale.title'), text: t('features.sezioni.personale.text') },
          { icon: 'shield-checkmark-outline', title: t('features.sezioni.permessi.title'), text: t('features.sezioni.permessi.text') },
          { icon: 'sparkles-outline', title: t('features.sezioni.assistente.title'), text: t('features.sezioni.assistente.text') }
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.Cumulativo, AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement],
        inclusi: [t('pricing.inclusi.anagraficheComuni'), t('pricing.inclusi.documentiAiClienti'), t('pricing.inclusi.exportPaghe'), t('pricing.inclusi.datiPronti')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.scelta.domanda'), risposta: t('pricing.domande.scelta.risposta') },
          { domanda: t('pricing.domande.dati.domanda'), risposta: t('pricing.domande.dati.risposta') },
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
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
          { icon: 'git-merge-outline', title: t('about.valori.integrato.title'), text: t('about.valori.integrato.text') },
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
