import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase di Risorse Umane: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo. Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseHumanResourcesService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi hr.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('hr.showcase.' + key, params) ?? 'hr.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'hr.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'id-card-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'cloud-upload-outline', title: t('home.punti.cedolini.title'), text: t('home.punti.cedolini.text') },
          { icon: 'checkmark-circle-outline', title: t('home.punti.presaVisione.title'), text: t('home.punti.presaVisione.text') },
          { icon: 'school-outline', title: t('home.punti.formazione.title'), text: t('home.punti.formazione.text') },
          { icon: 'list-outline', title: t('home.punti.checklist.title'), text: t('home.punti.checklist.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'people-outline', title: t('home.passi.carica.title'), text: t('home.passi.carica.text') },
          { icon: 'send-outline', title: t('home.passi.pubblica.title'), text: t('home.passi.pubblica.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('cloud-upload-outline', 'features.sezioni.cedolini'),
          v('person-outline', 'features.sezioni.portale'),
          v('school-outline', 'features.sezioni.formazione'),
          v('list-outline', 'features.sezioni.onboarding'),
          v('document-attach-outline', 'features.sezioni.consegne'),
          v('key-outline', 'features.sezioni.account'),
          v('briefcase-outline', 'features.sezioni.studio')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.HumanResources],
        inclusi: [t('pricing.inclusi.divisioneCedolini'), t('pricing.inclusi.presaVisione'), t('pricing.inclusi.modelliChecklist'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
          { domanda: t('pricing.domande.riconoscimento.domanda'), risposta: t('pricing.domande.riconoscimento.risposta') },
          { domanda: t('pricing.domande.senzaAccount.domanda'), risposta: t('pricing.domande.senzaAccount.risposta') },
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
          { icon: 'folder-open-outline', title: t('about.valori.ordinato.title'), text: t('about.valori.ordinato.text') },
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
