import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';
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
          { icon: 'folder-outline', title: t('features.sezioni.cartelle.title'), text: t('features.sezioni.cartelle.text') },
          { icon: 'pricetags-outline', title: t('features.sezioni.tipologie.title'), text: t('features.sezioni.tipologie.text') },
          { icon: 'id-card-outline', title: t('features.sezioni.personale.title'), text: t('features.sezioni.personale.text') },
          { icon: 'send-outline', title: t('features.sezioni.invioMultiplo.title'), text: t('features.sezioni.invioMultiplo.text') },
          { icon: 'checkmark-done-outline', title: t('features.sezioni.presaVisione.title'), text: t('features.sezioni.presaVisione.text') },
          { icon: 'git-branch-outline', title: t('features.sezioni.versioni.title'), text: t('features.sezioni.versioni.text') },
          { icon: 'search-outline', title: t('features.sezioni.ricerca.title'), text: t('features.sezioni.ricerca.text') },
          { icon: 'alarm-outline', title: t('features.sezioni.scadenze.title'), text: t('features.sezioni.scadenze.text') },
          { icon: 'key-outline', title: t('features.sezioni.abilitazioni.title'), text: t('features.sezioni.abilitazioni.text') },
          { icon: 'list-outline', title: t('features.sezioni.registro.title'), text: t('features.sezioni.registro.text') },
          { icon: 'trash-outline', title: t('features.sezioni.cestino.title'), text: t('features.sezioni.cestino.text') },
          { icon: 'pie-chart-outline', title: t('features.sezioni.spazio.title'), text: t('features.sezioni.spazio.text') }
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        piani: [
          { nome: t('pricing.piani.prova.nome'), prezzo: '0 €', periodo: t('pricing.piani.prova.periodo'), descrizione: t('pricing.piani.prova.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.caratteristiche.spazio1'), t('pricing.caratteristiche.tipologiePronte')], cta: t('pricing.piani.prova.cta') },
          { nome: t('pricing.piani.azienda.nome'), prezzo: '1 €', periodo: t('pricing.piani.azienda.periodo'), descrizione: t('pricing.piani.azienda.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.caratteristiche.spazio5'), t('pricing.caratteristiche.presaVisione'), t('pricing.caratteristiche.assistenzaEmailTelefono')], cta: t('pricing.piani.azienda.cta'), evidenza: true },
          { nome: t('pricing.piani.studio.nome'), prezzo: t('pricing.piani.studio.prezzo'), periodo: '', descrizione: t('pricing.piani.studio.descrizione'), caratteristiche: [t('pricing.caratteristiche.piuAziende'), t('pricing.caratteristiche.spazio20'), t('pricing.caratteristiche.invioMultiplo'), t('pricing.caratteristiche.assistenzaPrioritaria')], cta: t('pricing.piani.studio.cta'), url: '/contact' }
        ],
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
