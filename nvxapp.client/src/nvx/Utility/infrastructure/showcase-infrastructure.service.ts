import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem, MenuType } from './main-menu.service';
import { ShowcaseContenuti } from './showcase.model';
import { SITE_NAME } from './site-name';

/*
 Showcase generico di Infrastructure: voci di menu, rotte e testi delle pagine pubbliche
 (prima del login). Sono segnaposto: i rami applicativi li ridefiniscono con il proprio
 servizio (es. ShowcaseAttendanceTrackingService), vedi ShowcaseService.
*/
@Injectable({
  providedIn: 'root'
})
export class ShowcaseInfrastructureService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'showcase.menu.discover'          , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'showcase.menu.home'            , component: "ShowcaseHomePageComponent", url: '/home', icon: 'home' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'showcase.menu.features'    , component: "ShowcaseFeaturesPageComponent", url: '/features', icon: 'apps' },
      { menuType: MenuType.MenuItem,   zorder: 300, group: 0, title: 'showcase.menu.pricing'          , component: "ShowcasePricingPageComponent", url: '/pricing', icon: 'pricetags' },
      { menuType: MenuType.MenuItem,   zorder: 400, group: 0, title: 'showcase.menu.about'       , component: "ShowcaseAboutPageComponent", url: '/about', icon: 'people' },
      { menuType: MenuType.MenuItem,   zorder: 500, group: 0, title: 'showcase.menu.contact'        , component: "ShowcaseContactPageComponent", url: '/contact', icon: 'mail' },
      { menuType: MenuType.MenuHeader, zorder:   0, group: 1, title: 'showcase.menu.reservedArea'  , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 1, title: 'showcase.menu.signIn'          , component: "LoginPageComponent", url: '/login', icon: 'log-in' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 1, title: 'showcase.menu.register'      , component: "RegisterPageComponent", url: '/register', icon: 'person-add' },
      { menuType: MenuType.MenuItem,   zorder: 300, group: 1, title: 'showcase.menu.forgotPassword', component: "ForgotPasswordPageComponent", url: '/forgot-password', icon: 'key' }
    ];
  }

  // rotte delle pagine dello showcase ('login' resta di Infrastructure: e' anche fuori dallo showcase)
  public getRoutes(): Routes {
    return [
      { path: 'home', loadChildren: () => import('../../pages/infrastructure/showcase-home-page/showcase-home-page.module').then(m => m.ShowcaseHomePageModule) },
      { path: 'features', loadChildren: () => import('../../pages/infrastructure/showcase-features-page/showcase-features-page.module').then(m => m.ShowcaseFeaturesPageModule) },
      { path: 'pricing', loadChildren: () => import('../../pages/infrastructure/showcase-pricing-page/showcase-pricing-page.module').then(m => m.ShowcasePricingPageModule) },
      { path: 'about', loadChildren: () => import('../../pages/infrastructure/showcase-about-page/showcase-about-page.module').then(m => m.ShowcaseAboutPageModule) },
      { path: 'contact', loadChildren: () => import('../../pages/infrastructure/showcase-contact-page/showcase-contact-page.module').then(m => m.ShowcaseContactPageModule) },
      { path: 'register', loadChildren: () => import('../../pages/infrastructure/register-page/register-page.module').then(m => m.RegisterPageModule) },
      // link dell'email di conferma della registrazione (non e' una voce di menu)
      { path: 'confirm-email', loadChildren: () => import('../../pages/infrastructure/confirm-email-page/confirm-email-page.module').then(m => m.ConfirmEmailPageModule) },
      // recupero dell'accesso: richiesta del link e nuova password dal link dell'email
      { path: 'forgot-password', loadChildren: () => import('../../pages/infrastructure/forgot-password-page/forgot-password-page.module').then(m => m.ForgotPasswordPageModule) },
      { path: 'reset-password', loadChildren: () => import('../../pages/infrastructure/reset-password-page/reset-password-page.module').then(m => m.ResetPasswordPageModule) },
      // cambio della password conoscendo quella attuale (dal login, non e' una voce di menu)
      { path: 'change-password', loadChildren: () => import('../../pages/infrastructure/change-password-page/change-password-page.module').then(m => m.ChangePasswordPageModule) },
    ];
  }

  // testi tradotti (chiavi showcase.*): calcolati una volta, il cambio lingua ricarica la pagina
  private contenuti?: ShowcaseContenuti;

  public get Contenuti(): ShowcaseContenuti {
    return this.contenuti ??= this.creaContenuti();
  }

  private creaContenuti(): ShowcaseContenuti {
    const t = (key: string, params?: Record<string, unknown>) => this.transloco?.translate('showcase.' + key, params) ?? 'showcase.' + key;
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
        sottotitolo: t('home.sottotitolo', site),
        ctaPrimaria: t('home.ctaPrimaria'),
        ctaSecondaria: t('home.ctaSecondaria'),
        punti: [
          { icon: 'cloud-done-outline', title: t('home.punti.disponibile.title'), text: t('home.punti.disponibile.text') },
          { icon: 'shield-checkmark-outline', title: t('home.punti.sicuro.title'), text: t('home.punti.sicuro.text') },
          { icon: 'people-outline', title: t('home.punti.team.title'), text: t('home.punti.team.text') },
          { icon: 'sparkles-outline', title: t('home.punti.assistente.title'), text: t('home.punti.assistente.text') }
        ],
        passiTitolo: t('home.passiTitolo'),
        passi: [
          { icon: 'person-add-outline', title: t('home.passi.registrati.title'), text: t('home.passi.registrati.text') },
          { icon: 'options-outline', title: t('home.passi.configura.title'), text: t('home.passi.configura.text') },
          { icon: 'rocket-outline', title: t('home.passi.lavora.title'), text: t('home.passi.lavora.text') }
        ],
        chiusuraTitolo: t('home.chiusuraTitolo'),
        chiusuraTesto: t('home.chiusuraTesto')
      },
      features: {
        titolo: t('features.titolo'),
        sottotitolo: t('features.sottotitolo'),
        sezioni: [
          { icon: 'business-outline', title: t('features.sezioni.aziende.title'), text: t('features.sezioni.aziende.text') },
          { icon: 'key-outline', title: t('features.sezioni.ruoli.title'), text: t('features.sezioni.ruoli.text') },
          { icon: 'git-network-outline', title: t('features.sezioni.struttura.title'), text: t('features.sezioni.struttura.text') },
          { icon: 'swap-horizontal-outline', title: t('features.sezioni.impersonate.title'), text: t('features.sezioni.impersonate.text') },
          { icon: 'notifications-outline', title: t('features.sezioni.notifiche.title'), text: t('features.sezioni.notifiche.text') },
          { icon: 'phone-portrait-outline', title: t('features.sezioni.dispositivi.title'), text: t('features.sezioni.dispositivi.text') }
        ]
      },
      pricing: {
        titolo: t('pricing.titolo'),
        sottotitolo: t('pricing.sottotitolo'),
        piani: [
          { nome: t('pricing.piani.prova.nome'), prezzo: '0 €', periodo: t('pricing.piani.prova.periodo'), descrizione: t('pricing.piani.prova.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.piani.prova.utenti'), t('pricing.caratteristiche.assistenzaEmail')], cta: t('pricing.piani.prova.cta') },
          { nome: 'Standard', prezzo: '29 €', periodo: t('pricing.periodoMese'), descrizione: t('pricing.piani.standard.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.piani.standard.utenti'), t('pricing.piani.standard.assistenza')], cta: t('pricing.piani.standard.cta'), evidenza: true },
          { nome: 'Professional', prezzo: '79 €', periodo: t('pricing.periodoMese'), descrizione: t('pricing.piani.professional.descrizione'), caratteristiche: [t('pricing.caratteristiche.tutteLeFunzioni'), t('pricing.piani.professional.utenti'), t('pricing.piani.professional.assistenza'), t('pricing.piani.professional.aziende')], cta: t('pricing.piani.professional.cta') }
        ],
        nota: t('pricing.nota'),
        domande: [
          { domanda: t('pricing.domande.carta.domanda'), risposta: t('pricing.domande.carta.risposta') },
          { domanda: t('pricing.domande.utenti.domanda'), risposta: t('pricing.domande.utenti.risposta') },
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
          { icon: 'happy-outline', title: t('about.valori.semplicita.title'), text: t('about.valori.semplicita.text') },
          { icon: 'lock-closed-outline', title: t('about.valori.affidabilita.title'), text: t('about.valori.affidabilita.text') },
          { icon: 'chatbubbles-outline', title: t('about.valori.vicinanza.title'), text: t('about.valori.vicinanza.text') }
        ]
      },
      contact: {
        titolo: t('contact.titolo'),
        sottotitolo: t('contact.sottotitolo'),
        email: `info@${SITE_NAME.toLowerCase()}.it`,
        telefono: '+39 000 000 0000',
        indirizzo: 'Via Esempio 1, 00100 Roma',
        orari: t('contact.orari'),
        testoModulo: t('contact.testoModulo')
      },
      register: {
        titolo: t('register.titolo'),
        sottotitolo: t('register.sottotitolo'),
        vantaggi: [t('register.vantaggi.gratis'), t('register.vantaggi.nessunaCarta'), t('register.vantaggi.attiva')],
        condizioni: t('register.condizioni'),
        inviata: t('register.inviata'),
        confermaTitolo: t('register.confermaTitolo'),
        esito: t('register.esito')
      },
      passwordRecovery: {
        titolo: t('passwordRecovery.titolo'),
        sottotitolo: t('passwordRecovery.sottotitolo'),
        inviata: t('passwordRecovery.inviata'),
        resetTitolo: t('passwordRecovery.resetTitolo'),
        resetSottotitolo: t('passwordRecovery.resetSottotitolo'),
        resetFatto: t('passwordRecovery.resetFatto'),
        changeTitolo: t('passwordRecovery.changeTitolo'),
        changeSottotitolo: t('passwordRecovery.changeSottotitolo')
      }
    };
  }

}
