import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { MainMenuItem, MenuType } from './main-menu.service';
import { ShowcaseContenuti } from './showcase.model';

/*
 Showcase generico di Infrastructure: voci di menu, rotte e testi delle pagine pubbliche
 (prima del login). Sono segnaposto: i rami applicativi li ridefiniscono con il proprio
 servizio (es. ShowcaseAttendanceTrackingService), vedi ShowcaseService.
*/
@Injectable({
  providedIn: 'root'
})
export class ShowcaseInfrastructureService {

  constructor() { }

  public get Pages(): MainMenuItem[] {
    return [
      { menuType: MenuType.MenuHeader, zorder:   0, group: 0, title: 'Scopri'          , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 0, title: 'Home'            , component: "ShowcaseHomePageComponent", url: '/home', icon: 'home' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 0, title: 'Funzionalità'    , component: "ShowcaseFeaturesPageComponent", url: '/features', icon: 'apps' },
      { menuType: MenuType.MenuItem,   zorder: 300, group: 0, title: 'Prezzi'          , component: "ShowcasePricingPageComponent", url: '/pricing', icon: 'pricetags' },
      { menuType: MenuType.MenuItem,   zorder: 400, group: 0, title: 'Chi siamo'       , component: "ShowcaseAboutPageComponent", url: '/about', icon: 'people' },
      { menuType: MenuType.MenuItem,   zorder: 500, group: 0, title: 'Contatti'        , component: "ShowcaseContactPageComponent", url: '/contact', icon: 'mail' },
      { menuType: MenuType.MenuHeader, zorder:   0, group: 1, title: 'Area riservata'  , component: '', url: '', icon: '' },
      { menuType: MenuType.MenuItem,   zorder: 100, group: 1, title: 'Accedi'          , component: "LoginPageComponent", url: '/login', icon: 'log-in' },
      { menuType: MenuType.MenuItem,   zorder: 200, group: 1, title: 'Registrati'      , component: "RegisterPageComponent", url: '/register', icon: 'person-add' }
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
    ];
  }

  public get Contenuti(): ShowcaseContenuti {
    return {
      brand: {
        nome: 'SiSignore',
        icon: 'calendar-clear-outline',
        slogan: 'La piattaforma in cloud per far lavorare meglio la tua azienda.'
      },
      home: {
        etichetta: 'Gestionale in cloud',
        titolo: 'Tutto il lavoro della tua azienda, in un unico posto',
        sottotitolo: 'SiSignore riunisce utenti, processi e dati in una piattaforma semplice da usare, accessibile da computer, tablet e smartphone.',
        ctaPrimaria: 'Prova gratis',
        ctaSecondaria: 'Scopri le funzionalità',
        punti: [
          { icon: 'cloud-done-outline', title: 'Sempre disponibile', text: 'Nessuna installazione: accedi dal browser, ovunque ti trovi, con i dati sempre aggiornati.' },
          { icon: 'shield-checkmark-outline', title: 'Sicuro', text: 'Ogni azienda ha i suoi dati separati e ogni utente vede solo quello che gli serve.' },
          { icon: 'people-outline', title: 'Pensato per i team', text: 'Ruoli e permessi per amministratori e collaboratori, con un menu su misura per ciascuno.' },
          { icon: 'sparkles-outline', title: 'Assistente integrato', text: "Chiedi all'assistente cosa fare: ti guida tra le pagine e le funzioni." }
        ],
        passiTitolo: 'Si parte in tre passi',
        passi: [
          { icon: 'person-add-outline', title: 'Registrati', text: 'Crea la tua azienda in un minuto, senza carta di credito.' },
          { icon: 'options-outline', title: 'Configura', text: 'Aggiungi i collaboratori e scegli cosa può vedere ciascuno.' },
          { icon: 'rocket-outline', title: 'Lavora', text: 'Usa la piattaforma ogni giorno, da qualsiasi dispositivo.' }
        ],
        chiusuraTitolo: 'Pronto a iniziare?',
        chiusuraTesto: 'Registra la tua azienda e prova la piattaforma con i tuoi collaboratori.'
      },
      features: {
        titolo: 'Funzionalità',
        sottotitolo: 'Gli strumenti di base per organizzare la tua azienda, pronti fin dal primo accesso.',
        sezioni: [
          { icon: 'business-outline', title: 'Gestione aziende', text: 'Anagrafica dell’azienda e dei suoi utenti, con amministratori e collaboratori.' },
          { icon: 'key-outline', title: 'Ruoli e accessi', text: 'Ogni ruolo vede solo le sue pagine; l’amministratore decide quali voci di menu mostrare.' },
          { icon: 'git-network-outline', title: 'Studi e centri', text: 'Una struttura a più livelli: centri, studi professionali e aziende clienti.' },
          { icon: 'swap-horizontal-outline', title: 'Accesso per conto di', text: 'Chi assiste i clienti può entrare per conto di un utente e tornare indietro con un clic.' },
          { icon: 'notifications-outline', title: 'Notifiche in tempo reale', text: 'Avvisi e avanzamento delle elaborazioni lunghe, senza ricaricare la pagina.' },
          { icon: 'phone-portrait-outline', title: 'Su ogni dispositivo', text: 'Interfaccia unica per computer, tablet e smartphone, con tema chiaro e scuro.' }
        ]
      },
      pricing: {
        titolo: 'Prezzi',
        sottotitolo: 'Piani semplici, senza vincoli: inizi gratis e cresci quando ti serve.',
        piani: [
          { nome: 'Prova', prezzo: '0 €', periodo: 'per 30 giorni', descrizione: 'Per conoscere la piattaforma.', caratteristiche: ['Tutte le funzioni', 'Fino a 5 utenti', 'Assistenza via email'], cta: 'Inizia la prova' },
          { nome: 'Standard', prezzo: '29 €', periodo: 'al mese', descrizione: 'Per le piccole aziende.', caratteristiche: ['Tutte le funzioni', 'Fino a 25 utenti', 'Assistenza via email e telefono'], cta: 'Scegli Standard', evidenza: true },
          { nome: 'Professional', prezzo: '79 €', periodo: 'al mese', descrizione: 'Per aziende e studi strutturati.', caratteristiche: ['Tutte le funzioni', 'Utenti illimitati', 'Assistenza prioritaria', 'Più aziende gestite'], cta: 'Scegli Professional' }
        ],
        nota: 'Prezzi IVA esclusa. Puoi cambiare piano o disdire in qualsiasi momento.',
        domande: [
          { domanda: 'Serve una carta di credito per la prova?', risposta: 'No: la prova è gratuita e senza impegno.' },
          { domanda: 'Posso aggiungere utenti in seguito?', risposta: 'Sì, in qualsiasi momento; se superi i limiti del piano ti proponiamo quello adatto.' },
          { domanda: 'Dove sono conservati i miei dati?', risposta: 'In data center europei, separati per ogni azienda e salvati ogni giorno.' }
        ]
      },
      about: {
        titolo: 'Chi siamo',
        sottotitolo: 'Un team che costruisce software gestionale da anni, accanto ad aziende e professionisti.',
        paragrafi: [
          'Crediamo che il software gestionale debba essere semplice: poche pagine chiare, che fanno bene il loro lavoro.',
          'Per questo sviluppiamo SiSignore insieme a chi lo usa ogni giorno, aggiungendo funzioni a partire dalle esigenze reali.'
        ],
        valori: [
          { icon: 'happy-outline', title: 'Semplicità', text: 'Ogni funzione deve essere chiara al primo utilizzo.' },
          { icon: 'lock-closed-outline', title: 'Affidabilità', text: 'I dati dei clienti sono al sicuro e sempre disponibili.' },
          { icon: 'chatbubbles-outline', title: 'Vicinanza', text: 'Rispondiamo alle domande e ascoltiamo i suggerimenti.' }
        ]
      },
      contact: {
        titolo: 'Contatti',
        sottotitolo: 'Hai domande o vuoi una dimostrazione? Scrivici, ti rispondiamo entro un giorno lavorativo.',
        email: 'info@sisignore.it',
        telefono: '+39 000 000 0000',
        indirizzo: 'Via Esempio 1, 00100 Roma',
        orari: 'Lunedì - venerdì, 9:00 - 18:00',
        testoModulo: 'Compila il modulo: si aprirà il tuo programma di posta con il messaggio pronto da inviare.'
      },
      register: {
        titolo: 'Registra la tua azienda',
        sottotitolo: 'Crea l’account della tua azienda e inizia subito a usare la piattaforma.',
        vantaggi: ['Gratis per 30 giorni', 'Nessuna carta di credito', 'Attiva in un minuto'],
        condizioni: 'Accetto le condizioni del servizio e l’informativa sulla privacy',
        esito: 'La tua azienda è pronta. Questi sono gli utenti creati: accedi con la password che hai scelto.'
      }
    };
  }

}
