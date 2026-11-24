import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase di Compliance: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo (sicurezza sul lavoro e registro GDPR). Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseComplianceService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi cmp.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('cmp.showcase.' + key, params) ?? 'cmp.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'cmp.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'shield-checkmark-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'school-outline', title: t('home.punti.formazione.title'), text: t('home.punti.formazione.text') },
          { icon: 'medkit-outline', title: t('home.punti.visite.title'), text: t('home.punti.visite.text') },
          { icon: 'lock-closed-outline', title: t('home.punti.gdpr.title'), text: t('home.punti.gdpr.text') },
          { icon: 'alarm-outline', title: t('home.punti.scadenze.title'), text: t('home.punti.scadenze.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'construct-outline', title: t('home.passi.configura.title'), text: t('home.passi.configura.text') },
          { icon: 'checkmark-done-outline', title: t('home.passi.controlla.title'), text: t('home.passi.controlla.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('school-outline', 'features.sezioni.corsi'),
          v('medkit-outline', 'features.sezioni.visite'),
          v('hand-left-outline', 'features.sezioni.dpi'),
          v('ribbon-outline', 'features.sezioni.nomine'),
          v('lock-closed-outline', 'features.sezioni.gdpr'),
          v('warning-outline', 'features.sezioni.violazioni'),
          v('alarm-outline', 'features.sezioni.scadenze'),
          v('person-outline', 'features.sezioni.adempimenti'),
          v('mic-outline', 'features.sezioni.vocale'),
          v('briefcase-outline', 'features.sezioni.studio')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.Compliance],
        inclusi: [t('pricing.inclusi.sicurezza'), t('pricing.inclusi.gdpr'), t('pricing.inclusi.scadenzario'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.sanitari.domanda'), risposta: t('pricing.domande.sanitari.risposta') },
          { domanda: t('pricing.domande.dipendenti.domanda'), risposta: t('pricing.domande.dipendenti.risposta') },
          { domanda: t('pricing.domande.export.domanda'), risposta: t('pricing.domande.export.risposta') },
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
          { icon: 'happy-outline', title: t('about.valori.chiaro.title'), text: t('about.valori.chiaro.text') },
          { icon: 'shield-checkmark-outline', title: t('about.valori.riservato.title'), text: t('about.valori.riservato.text') },
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
