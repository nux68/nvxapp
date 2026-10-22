import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';
import { SITE_NAME } from './site-name';

// Showcase della gestione presenze: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo. Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAttendanceTrackingService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  // testi tradotti (chiavi attendance.showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenutiRidefiniti;

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenutiRidefiniti {
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco?.translate('attendance.showcase.' + key, params) ?? 'attendance.showcase.' + key;
    const site = { siteName: SITE_NAME };
    return {
      brand: {
        nome: SITE_NAME,
        icon: 'calendar-clear-outline',
        slogan: t('brand.slogan')
      },
      home: {
        etichetta: t('home.etichetta'),
        titolo: t('home.titolo'),
        sottotitolo: t('home.sottotitolo'),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'finger-print-outline', title: t('home.punti.timbratura.title'), text: t('home.punti.timbratura.text') },
          { icon: 'checkmark-done-outline', title: t('home.punti.richieste.title'), text: t('home.punti.richieste.text') },
          { icon: 'calendar-number-outline', title: t('home.punti.cartellino.title'), text: t('home.punti.cartellino.text') },
          { icon: 'receipt-outline', title: t('home.punti.noteSpese.title'), text: t('home.punti.noteSpese.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registra.title'), text: t('home.passi.registra.text') },
          { icon: 'git-network-outline', title: t('home.passi.organizza.title'), text: t('home.passi.organizza.text') },
          { icon: 'cloud-upload-outline', title: t('home.passi.chiudi.title'), text: t('home.passi.chiudi.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          { icon: 'mic-outline', title: t('features.sezioni.vocale.title'), text: t('features.sezioni.vocale.text') },
          { icon: 'finger-print-outline', title: t('features.sezioni.timbrature.title'), text: t('features.sezioni.timbrature.text') },
          { icon: 'document-text-outline', title: t('features.sezioni.giustificativi.title'), text: t('features.sezioni.giustificativi.text') },
          { icon: 'time-outline', title: t('features.sezioni.straordinari.title'), text: t('features.sezioni.straordinari.text') },
          { icon: 'people-circle-outline', title: t('features.sezioni.reparti.title'), text: t('features.sezioni.reparti.text') },
          { icon: 'calendar-outline', title: t('features.sezioni.calendario.title'), text: t('features.sezioni.calendario.text') },
          { icon: 'pulse-outline', title: t('features.sezioni.presenti.title'), text: t('features.sezioni.presenti.text') },
          { icon: 'speedometer-outline', title: t('features.sezioni.profili.title'), text: t('features.sezioni.profili.text') },
          { icon: 'checkbox-outline', title: t('features.sezioni.quadratura.title'), text: t('features.sezioni.quadratura.text') },
          { icon: 'download-outline', title: t('features.sezioni.export.title'), text: t('features.sezioni.export.text') },
          { icon: 'receipt-outline', title: t('features.sezioni.noteSpese.title'), text: t('features.sezioni.noteSpese.text') },
          { icon: 'briefcase-outline', title: t('features.sezioni.commesse.title'), text: t('features.sezioni.commesse.text') },
          { icon: 'options-outline', title: t('features.sezioni.menu.title'), text: t('features.sezioni.menu.text') }
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        piani: [
          { nome: t('pricing.piani.prova.nome'), prezzo: '0 €', periodo: t('pricing.piani.prova.periodo'), descrizione: t('pricing.piani.prova.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.caratteristiche.fino10Dipendenti'), t('pricing.caratteristiche.causaliPronte')], cta: t('pricing.piani.prova.cta') },
          { nome: t('pricing.piani.azienda.nome'), prezzo: '2 €', periodo: t('pricing.piani.azienda.periodo'), descrizione: t('pricing.piani.azienda.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.caratteristiche.timbraturaSmartphone'), t('pricing.caratteristiche.exportPaghe'), t('pricing.caratteristiche.assistenzaEmailTelefono')], cta: t('pricing.piani.azienda.cta'), evidenza: true },
          { nome: t('pricing.piani.studio.nome'), prezzo: t('pricing.piani.studio.prezzo'), periodo: '', descrizione: t('pricing.piani.studio.descrizione'), caratteristiche: [t('pricing.caratteristiche.piuAziende'), t('pricing.caratteristiche.accessoPerCliente'), t('pricing.caratteristiche.modelliExport'), t('pricing.caratteristiche.assistenzaPrioritaria')], cta: t('pricing.piani.studio.cta'), url: '/contact' }
        ],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.installare.domanda'), risposta: t('pricing.domande.installare.risposta') },
          { domanda: t('pricing.domande.paghe.domanda'), risposta: t('pricing.domande.paghe.risposta') },
          { domanda: t('pricing.domande.approvazione.domanda'), risposta: t('pricing.domande.approvazione.risposta') },
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
          { icon: 'shield-checkmark-outline', title: t('about.valori.preciso.title'), text: t('about.valori.preciso.text') },
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
