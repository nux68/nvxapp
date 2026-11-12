import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';
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
              { icon: 'finger-print-outline', title: t('features.gruppi.presenze.voci.timbrature.title'), text: t('features.gruppi.presenze.voci.timbrature.text') },
              { icon: 'calendar-number-outline', title: t('features.gruppi.presenze.voci.turni.title'), text: t('features.gruppi.presenze.voci.turni.text') },
              { icon: 'home-outline', title: t('features.gruppi.presenze.voci.smartWorking.title'), text: t('features.gruppi.presenze.voci.smartWorking.text') },
              { icon: 'checkmark-done-outline', title: t('features.gruppi.presenze.voci.richieste.title'), text: t('features.gruppi.presenze.voci.richieste.text') },
              { icon: 'receipt-outline', title: t('features.gruppi.presenze.voci.noteSpese.title'), text: t('features.gruppi.presenze.voci.noteSpese.text') },
              { icon: 'alert-circle-outline', title: t('features.gruppi.presenze.voci.anomalie.title'), text: t('features.gruppi.presenze.voci.anomalie.text') },
              { icon: 'bar-chart-outline', title: t('features.gruppi.presenze.voci.commesse.title'), text: t('features.gruppi.presenze.voci.commesse.text') },
              { icon: 'download-outline', title: t('features.gruppi.presenze.voci.paghe.title'), text: t('features.gruppi.presenze.voci.paghe.text') }
            ]
          },
          {
            titolo: t('features.gruppi.crm.titolo'), sottotitolo: t('features.gruppi.crm.sottotitolo'), icon: 'git-network-outline',
            sezioni: [
              { icon: 'business-outline', title: t('features.gruppi.crm.voci.clienti.title'), text: t('features.gruppi.crm.voci.clienti.text') },
              { icon: 'funnel-outline', title: t('features.gruppi.crm.voci.pipeline.title'), text: t('features.gruppi.crm.voci.pipeline.text') },
              { icon: 'calendar-outline', title: t('features.gruppi.crm.voci.agenda.title'), text: t('features.gruppi.crm.voci.agenda.text') },
              { icon: 'document-text-outline', title: t('features.gruppi.crm.voci.preventivi.title'), text: t('features.gruppi.crm.voci.preventivi.text') },
              { icon: 'headset-outline', title: t('features.gruppi.crm.voci.assistenza.title'), text: t('features.gruppi.crm.voci.assistenza.text') },
              { icon: 'stats-chart-outline', title: t('features.gruppi.crm.voci.report.title'), text: t('features.gruppi.crm.voci.report.text') },
              { icon: 'eye-outline', title: t('features.gruppi.crm.voci.visibilita.title'), text: t('features.gruppi.crm.voci.visibilita.text') }
            ]
          },
          {
            titolo: t('features.gruppi.documentale.titolo'), sottotitolo: t('features.gruppi.documentale.sottotitolo'), icon: 'documents-outline',
            sezioni: [
              { icon: 'folder-open-outline', title: t('features.gruppi.documentale.voci.archivio.title'), text: t('features.gruppi.documentale.voci.archivio.text') },
              { icon: 'search-outline', title: t('features.gruppi.documentale.voci.ricerca.title'), text: t('features.gruppi.documentale.voci.ricerca.text') },
              { icon: 'people-outline', title: t('features.gruppi.documentale.voci.personale.title'), text: t('features.gruppi.documentale.voci.personale.text') },
              { icon: 'create-outline', title: t('features.gruppi.documentale.voci.firma.title'), text: t('features.gruppi.documentale.voci.firma.text') },
              { icon: 'clipboard-outline', title: t('features.gruppi.documentale.voci.verbali.title'), text: t('features.gruppi.documentale.voci.verbali.text') },
              { icon: 'lock-closed-outline', title: t('features.gruppi.documentale.voci.riservatezza.title'), text: t('features.gruppi.documentale.voci.riservatezza.text') },
              { icon: 'server-outline', title: t('features.gruppi.documentale.voci.spazio.title'), text: t('features.gruppi.documentale.voci.spazio.text') }
            ]
          },
          {
            titolo: t('features.gruppi.risorseUmane.titolo'), sottotitolo: t('features.gruppi.risorseUmane.sottotitolo'), icon: 'id-card-outline',
            sezioni: [
              { icon: 'cloud-upload-outline', title: t('features.gruppi.risorseUmane.voci.cedolini.title'), text: t('features.gruppi.risorseUmane.voci.cedolini.text') },
              { icon: 'person-outline', title: t('features.gruppi.risorseUmane.voci.portale.title'), text: t('features.gruppi.risorseUmane.voci.portale.text') },
              { icon: 'school-outline', title: t('features.gruppi.risorseUmane.voci.formazione.title'), text: t('features.gruppi.risorseUmane.voci.formazione.text') },
              { icon: 'list-outline', title: t('features.gruppi.risorseUmane.voci.onboarding.title'), text: t('features.gruppi.risorseUmane.voci.onboarding.text') },
              { icon: 'document-attach-outline', title: t('features.gruppi.risorseUmane.voci.consegne.title'), text: t('features.gruppi.risorseUmane.voci.consegne.text') },
              { icon: 'key-outline', title: t('features.gruppi.risorseUmane.voci.account.title'), text: t('features.gruppi.risorseUmane.voci.account.text') }
            ]
          },
          {
            titolo: t('features.gruppi.compliance.titolo'), sottotitolo: t('features.gruppi.compliance.sottotitolo'), icon: 'shield-checkmark-outline',
            sezioni: [
              { icon: 'school-outline', title: t('features.gruppi.compliance.voci.corsi.title'), text: t('features.gruppi.compliance.voci.corsi.text') },
              { icon: 'medkit-outline', title: t('features.gruppi.compliance.voci.visite.title'), text: t('features.gruppi.compliance.voci.visite.text') },
              { icon: 'hand-left-outline', title: t('features.gruppi.compliance.voci.dpi.title'), text: t('features.gruppi.compliance.voci.dpi.text') },
              { icon: 'ribbon-outline', title: t('features.gruppi.compliance.voci.nomine.title'), text: t('features.gruppi.compliance.voci.nomine.text') },
              { icon: 'lock-closed-outline', title: t('features.gruppi.compliance.voci.gdpr.title'), text: t('features.gruppi.compliance.voci.gdpr.text') },
              { icon: 'warning-outline', title: t('features.gruppi.compliance.voci.violazioni.title'), text: t('features.gruppi.compliance.voci.violazioni.text') },
              { icon: 'briefcase-outline', title: t('features.gruppi.compliance.voci.studio.title'), text: t('features.gruppi.compliance.voci.studio.text') }
            ]
          },
          {
            titolo: t('features.gruppi.risorseAziendali.titolo'), sottotitolo: t('features.gruppi.risorseAziendali.sottotitolo'), icon: 'cube-outline',
            sezioni: [
              { icon: 'car-outline', title: t('features.gruppi.risorseAziendali.voci.flotta.title'), text: t('features.gruppi.risorseAziendali.voci.flotta.text') },
              { icon: 'alarm-outline', title: t('features.gruppi.risorseAziendali.voci.scadenzeVeicoli.title'), text: t('features.gruppi.risorseAziendali.voci.scadenzeVeicoli.text') },
              { icon: 'cash-outline', title: t('features.gruppi.risorseAziendali.voci.multe.title'), text: t('features.gruppi.risorseAziendali.voci.multe.text') },
              { icon: 'laptop-outline', title: t('features.gruppi.risorseAziendali.voci.beni.title'), text: t('features.gruppi.risorseAziendali.voci.beni.text') },
              { icon: 'swap-horizontal-outline', title: t('features.gruppi.risorseAziendali.voci.consegne.title'), text: t('features.gruppi.risorseAziendali.voci.consegne.text') },
              { icon: 'today-outline', title: t('features.gruppi.risorseAziendali.voci.prenotazioni.title'), text: t('features.gruppi.risorseAziendali.voci.prenotazioni.text') }
            ]
          },
          {
            titolo: t('features.gruppi.comuni.titolo'), sottotitolo: t('features.gruppi.comuni.sottotitolo'), icon: 'apps-outline',
            sezioni: [
              { icon: 'alarm-outline', title: t('features.gruppi.comuni.voci.scadenzario.title'), text: t('features.gruppi.comuni.voci.scadenzario.text') },
              { icon: 'notifications-outline', title: t('features.gruppi.comuni.voci.notifiche.title'), text: t('features.gruppi.comuni.voci.notifiche.text') },
              { icon: 'checkmark-circle-outline', title: t('features.gruppi.comuni.voci.preseVisione.title'), text: t('features.gruppi.comuni.voci.preseVisione.text') },
              { icon: 'calendar-outline', title: t('features.gruppi.comuni.voci.calendario.title'), text: t('features.gruppi.comuni.voci.calendario.text') },
              { icon: 'sparkles-outline', title: t('features.gruppi.comuni.voci.riepilogo.title'), text: t('features.gruppi.comuni.voci.riepilogo.text') },
              { icon: 'mic-outline', title: t('features.gruppi.comuni.voci.assistente.title'), text: t('features.gruppi.comuni.voci.assistente.text') },
              { icon: 'help-buoy-outline', title: t('features.gruppi.comuni.voci.ticket.title'), text: t('features.gruppi.comuni.voci.ticket.text') }
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
