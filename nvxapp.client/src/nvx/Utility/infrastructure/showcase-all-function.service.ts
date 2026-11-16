import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti, voceShowcase } from './showcase.model';
import { AmbitoLicenza } from '../../ClientServer-Service/Infrastructure/License/Models/license-model';
import { SITE_NAME } from './site-name';

// Showcase dell'applicativo completo (ramo AllFunction): tutti gli applicativi insieme, sulle stesse pagine template
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
    // voce della pagina funzionalita' con l'eventuale dettaglio (<chiave>.dettaglio), aperto in un dialog
    const v = (icon: string, key: string) => voceShowcase(this.transloco, icon, 'allfunction.showcase.' + key);
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
          { icon: 'id-card-outline', title: t('home.punti.risorseUmane.title'), text: t('home.punti.risorseUmane.text') },
          { icon: 'shield-checkmark-outline', title: t('home.punti.compliance.title'), text: t('home.punti.compliance.text') },
          { icon: 'cube-outline', title: t('home.punti.risorseAziendali.title'), text: t('home.punti.risorseAziendali.text') },
          { icon: 'sparkles-outline', title: t('home.punti.comuni.title'), text: t('home.punti.comuni.text') },
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
        // le funzionalita' sono nei gruppi (uno per applicativo, piu' le funzioni comuni)
        sezioni: [],
        gruppi: [
          {
            titolo: t('features.gruppi.presenze.titolo'), sottotitolo: t('features.gruppi.presenze.sottotitolo'), icon: 'calendar-clear-outline',
            sezioni: [
              v('finger-print-outline', 'features.gruppi.presenze.voci.timbrature'),
              v('calendar-number-outline', 'features.gruppi.presenze.voci.turni'),
              v('home-outline', 'features.gruppi.presenze.voci.smartWorking'),
              v('checkmark-done-outline', 'features.gruppi.presenze.voci.richieste'),
              v('receipt-outline', 'features.gruppi.presenze.voci.noteSpese'),
              v('alert-circle-outline', 'features.gruppi.presenze.voci.anomalie'),
              v('bar-chart-outline', 'features.gruppi.presenze.voci.commesse'),
              v('download-outline', 'features.gruppi.presenze.voci.paghe')
            ]
          },
          {
            titolo: t('features.gruppi.crm.titolo'), sottotitolo: t('features.gruppi.crm.sottotitolo'), icon: 'git-network-outline',
            sezioni: [
              v('business-outline', 'features.gruppi.crm.voci.clienti'),
              v('funnel-outline', 'features.gruppi.crm.voci.pipeline'),
              v('calendar-outline', 'features.gruppi.crm.voci.agenda'),
              v('document-text-outline', 'features.gruppi.crm.voci.preventivi'),
              v('headset-outline', 'features.gruppi.crm.voci.assistenza'),
              v('stats-chart-outline', 'features.gruppi.crm.voci.report'),
              v('eye-outline', 'features.gruppi.crm.voci.visibilita')
            ]
          },
          {
            titolo: t('features.gruppi.documentale.titolo'), sottotitolo: t('features.gruppi.documentale.sottotitolo'), icon: 'documents-outline',
            sezioni: [
              v('folder-open-outline', 'features.gruppi.documentale.voci.archivio'),
              v('search-outline', 'features.gruppi.documentale.voci.ricerca'),
              v('people-outline', 'features.gruppi.documentale.voci.personale'),
              v('create-outline', 'features.gruppi.documentale.voci.firma'),
              v('clipboard-outline', 'features.gruppi.documentale.voci.verbali'),
              v('lock-closed-outline', 'features.gruppi.documentale.voci.riservatezza'),
              v('server-outline', 'features.gruppi.documentale.voci.spazio')
            ]
          },
          {
            titolo: t('features.gruppi.risorseUmane.titolo'), sottotitolo: t('features.gruppi.risorseUmane.sottotitolo'), icon: 'id-card-outline',
            sezioni: [
              v('cloud-upload-outline', 'features.gruppi.risorseUmane.voci.cedolini'),
              v('person-outline', 'features.gruppi.risorseUmane.voci.portale'),
              v('school-outline', 'features.gruppi.risorseUmane.voci.formazione'),
              v('list-outline', 'features.gruppi.risorseUmane.voci.onboarding'),
              v('document-attach-outline', 'features.gruppi.risorseUmane.voci.consegne'),
              v('key-outline', 'features.gruppi.risorseUmane.voci.account')
            ]
          },
          {
            titolo: t('features.gruppi.compliance.titolo'), sottotitolo: t('features.gruppi.compliance.sottotitolo'), icon: 'shield-checkmark-outline',
            sezioni: [
              v('school-outline', 'features.gruppi.compliance.voci.corsi'),
              v('medkit-outline', 'features.gruppi.compliance.voci.visite'),
              v('hand-left-outline', 'features.gruppi.compliance.voci.dpi'),
              v('ribbon-outline', 'features.gruppi.compliance.voci.nomine'),
              v('lock-closed-outline', 'features.gruppi.compliance.voci.gdpr'),
              v('warning-outline', 'features.gruppi.compliance.voci.violazioni'),
              v('briefcase-outline', 'features.gruppi.compliance.voci.studio')
            ]
          },
          {
            titolo: t('features.gruppi.risorseAziendali.titolo'), sottotitolo: t('features.gruppi.risorseAziendali.sottotitolo'), icon: 'cube-outline',
            sezioni: [
              v('car-outline', 'features.gruppi.risorseAziendali.voci.flotta'),
              v('alarm-outline', 'features.gruppi.risorseAziendali.voci.scadenzeVeicoli'),
              v('cash-outline', 'features.gruppi.risorseAziendali.voci.multe'),
              v('laptop-outline', 'features.gruppi.risorseAziendali.voci.beni'),
              v('swap-horizontal-outline', 'features.gruppi.risorseAziendali.voci.consegne'),
              v('today-outline', 'features.gruppi.risorseAziendali.voci.prenotazioni')
            ]
          },
          {
            titolo: t('features.gruppi.comuni.titolo'), sottotitolo: t('features.gruppi.comuni.sottotitolo'), icon: 'apps-outline',
            sezioni: [
              v('alarm-outline', 'features.gruppi.comuni.voci.scadenzario'),
              v('notifications-outline', 'features.gruppi.comuni.voci.notifiche'),
              v('checkmark-circle-outline', 'features.gruppi.comuni.voci.preseVisione'),
              v('calendar-outline', 'features.gruppi.comuni.voci.calendario'),
              v('sparkles-outline', 'features.gruppi.comuni.voci.riepilogo'),
              v('mic-outline', 'features.gruppi.comuni.voci.assistente'),
              v('help-buoy-outline', 'features.gruppi.comuni.voci.ticket')
            ]
          }
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        ambiti: [AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement,
          AmbitoLicenza.HumanResources, AmbitoLicenza.Compliance, AmbitoLicenza.Assets, AmbitoLicenza.Cumulativo],
        inclusi: [
          t('pricing.inclusi.anagraficheComuni'),
          t('pricing.inclusi.scadenzario'),
          t('pricing.inclusi.preseVisione'),
          t('pricing.inclusi.assistente'),
          t('pricing.inclusi.calendario'),
          t('pricing.inclusi.datiPronti')
        ],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.scelta.domanda'), risposta: t('pricing.domande.scelta.risposta') },
          { domanda: t('pricing.domande.dati.domanda'), risposta: t('pricing.domande.dati.risposta') },
          { domanda: t('pricing.domande.collegamenti.domanda'), risposta: t('pricing.domande.collegamenti.risposta') },
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
