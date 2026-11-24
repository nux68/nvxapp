import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase del documentale: stesse pagine template di Infrastructure (ShowcaseService), con i testi sulle funzioni
// del ramo (chiavi documents.showcase.*). Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseDocumentManagementService {

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
      this.transloco?.translate('documents.showcase.' + key, params) ?? 'documents.showcase.' + key;
    const site = { siteName: SITE_NAME };
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'documents.showcase.' + key);
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'folder-open-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'documents-outline', title: t('home.punti.archivio.title'), text: t('home.punti.archivio.text') },
          { icon: 'people-outline', title: t('home.punti.personale.title'), text: t('home.punti.personale.text') },
          { icon: 'checkmark-done-outline', title: t('home.punti.presaVisione.title'), text: t('home.punti.presaVisione.text') },
          { icon: 'search-outline', title: t('home.punti.ricerca.title'), text: t('home.punti.ricerca.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'cloud-upload-outline', title: t('home.passi.carica.title'), text: t('home.passi.carica.text') },
          { icon: 'send-outline', title: t('home.passi.consegna.title'), text: t('home.passi.consegna.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          v('folder-outline', 'features.sezioni.cartelle'),
          v('pricetags-outline', 'features.sezioni.tipologie'),
          v('id-card-outline', 'features.sezioni.personale'),
          v('send-outline', 'features.sezioni.invioMultiplo'),
          v('checkmark-done-outline', 'features.sezioni.presaVisione'),
          v('git-branch-outline', 'features.sezioni.versioni'),
          v('search-outline', 'features.sezioni.ricerca'),
          v('alarm-outline', 'features.sezioni.scadenze'),
          v('key-outline', 'features.sezioni.abilitazioni'),
          v('list-outline', 'features.sezioni.registro'),
          v('trash-outline', 'features.sezioni.cestino'),
          v('pie-chart-outline', 'features.sezioni.spazio')
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.DocumentManagement],
        inclusi: [t('pricing.inclusi.presaVisione'), t('pricing.inclusi.invioMultiplo'), t('pricing.inclusi.tipologiePronte'), t('pricing.inclusi.tutteLeFunzioni')],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.sicurezza.domanda'), risposta: t('pricing.domande.sicurezza.risposta') },
          { domanda: t('pricing.domande.cedolini.domanda'), risposta: t('pricing.domande.cedolini.risposta') },
          { domanda: t('pricing.domande.spazio.domanda'), risposta: t('pricing.domande.spazio.risposta') },
          { domanda: t('pricing.domande.senzaAccount.domanda'), risposta: t('pricing.domande.senzaAccount.risposta') }
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
          { icon: 'lock-closed-outline', title: t('about.valori.riservato.title'), text: t('about.valori.riservato.text') },
          { icon: 'happy-outline', title: t('about.valori.semplice.title'), text: t('about.valori.semplice.text') },
          { icon: 'shield-checkmark-outline', title: t('about.valori.tracciato.title'), text: t('about.valori.tracciato.text') }
        ]
      },
      register: {
        titolo: t('register.titolo'),
        sottotitolo: t('register.sottotitolo'),
        vantaggi: [t('register.vantaggi.gratis'), t('register.vantaggi.attivo'), t('register.vantaggi.nessunaCarta')],
        condizioni: t('register.condizioni'),
        inviata: t('register.inviata'),
        confermaTitolo: t('register.confermaTitolo'),
        esito: t('register.esito')
      }
    };
  }
}
