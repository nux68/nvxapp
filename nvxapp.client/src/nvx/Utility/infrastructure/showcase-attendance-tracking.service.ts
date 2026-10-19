import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';

// Showcase della gestione presenze: stesse pagine template di Infrastructure (ShowcaseService),
// con i testi sulle funzioni del ramo. Le voci di menu e le rotte restano quelle generiche.
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAttendanceTrackingService {

  constructor() { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }

  public get Contenuti(): ShowcaseContenutiRidefiniti {
    return {
      brand: {
        nome: 'SiSignore',
        icon: 'calendar-clear-outline',
        slogan: 'Presenze, richieste e note spese della tua azienda, in cloud.'
      },
      home: {
        etichetta: 'Gestione presenze in cloud',
        titolo: 'Le presenze dei tuoi dipendenti, senza fogli e senza rincorse',
        sottotitolo: 'Timbrature, ferie, permessi, straordinari e note spese in un’unica app: i dipendenti chiedono, i responsabili approvano, a fine mese il cartellino è pronto per le paghe.',
        ctaPrimaria: 'Prova gratis',
        ctaSecondaria: 'Scopri le funzionalità',
        punti: [
          { icon: 'finger-print-outline', title: 'Timbratura da ovunque', text: 'Entrata e uscita dallo smartphone o dal computer; le correzioni passano da una richiesta.' },
          { icon: 'checkmark-done-outline', title: 'Richieste e approvazioni', text: 'Ferie, permessi e giustificativi arrivano al responsabile del reparto, che approva o rifiuta con un clic.' },
          { icon: 'calendar-number-outline', title: 'Cartellino sempre aggiornato', text: 'Ore lavorate, assenze e straordinari calcolati ogni giorno secondo il profilo orario di ognuno.' },
          { icon: 'receipt-outline', title: 'Note spese', text: 'Rimborsi con scontrini e fatture allegati, approvati insieme alla nota.' }
        ],
        passiTitolo: 'Operativi in tre passi',
        passi: [
          { icon: 'person-add-outline', title: 'Registra l’azienda', text: 'La gestione presenze è attiva da subito, con causali e parametri già pronti.' },
          { icon: 'git-network-outline', title: 'Organizza', text: 'Sedi, reparti, orari e approvatori: ogni richiesta arriva alla persona giusta.' },
          { icon: 'cloud-upload-outline', title: 'Chiudi il mese', text: 'Quadratura e approvazione dei cartellini, poi l’export delle causali per le paghe.' }
        ],
        chiusuraTitolo: 'Basta fogli presenze',
        chiusuraTesto: 'Registra la tua azienda e prova la gestione presenze con i tuoi dipendenti.'
      },
      features: {
        titolo: 'Funzionalità',
        sottotitolo: 'Tutto quello che serve per gestire presenze e assenze, dalla timbratura all’export per le paghe.',
        sezioni: [
          { icon: 'finger-print-outline', title: 'Timbrature', text: 'Timbratura di entrata e uscita e richiesta di correzione delle timbrature dimenticate o errate.' },
          { icon: 'document-text-outline', title: 'Giustificativi', text: 'Ferie, permessi, malattia e le altre causali, chieste dal dipendente e approvate dal responsabile.' },
          { icon: 'time-outline', title: 'Straordinari e banca ore', text: 'Straordinario autorizzato con giustificativo e ore in più accreditate in banca ore nei cartellini approvati.' },
          { icon: 'people-circle-outline', title: 'Reparti e approvatori', text: 'Per ogni reparto chi amministra e chi approva le richieste, con la modalità di approvazione scelta dall’azienda.' },
          { icon: 'calendar-outline', title: 'Calendario e piano ferie', text: 'Il calendario di ogni dipendente, il calendario HR e il piano ferie di tutta l’azienda.' },
          { icon: 'pulse-outline', title: 'Personale presente', text: 'Chi è al lavoro, chi è assente e perché, in tempo reale.' },
          { icon: 'speedometer-outline', title: 'Profili orari', text: 'Orari, festività e sedi: il cartellino si calcola sulle regole di ciascuno.' },
          { icon: 'checkbox-outline', title: 'Quadratura mensile', text: 'Controllo dei cartellini del mese, anomalie in evidenza e approvazione in blocco.' },
          { icon: 'download-outline', title: 'Export per le paghe', text: 'Export delle causali con modelli configurabili, pronto per il programma paghe.' },
          { icon: 'receipt-outline', title: 'Note spese', text: 'Rimborsi con più spese e allegati; l’approvazione della nota approva tutte le spese.' },
          { icon: 'briefcase-outline', title: 'Commesse e attività', text: 'Clienti, commesse e attività per sapere su cosa si lavora, con le statistiche.' },
          { icon: 'options-outline', title: 'Menu su misura', text: 'L’amministratore sceglie quali pagine vedono gli amministratori e i dipendenti.' }
        ]
      },
      pricing: {
        titolo: 'Prezzi',
        sottotitolo: 'Paghi per i dipendenti che gestisci: tutte le funzioni sono sempre incluse.',
        piani: [
          { nome: 'Prova', prezzo: '0 €', periodo: 'per 30 giorni', descrizione: 'Per provare con i tuoi dipendenti.', caratteristiche: ['Tutte le funzioni', 'Fino a 10 dipendenti', 'Causali e parametri pronti'], cta: 'Inizia la prova' },
          { nome: 'Azienda', prezzo: '2 €', periodo: 'per dipendente al mese', descrizione: 'Per piccole e medie imprese.', caratteristiche: ['Tutte le funzioni', 'Timbratura da smartphone', 'Export per le paghe', 'Assistenza via email e telefono'], cta: 'Scegli Azienda', evidenza: true },
          { nome: 'Studio', prezzo: 'Su misura', periodo: '', descrizione: 'Per consulenti del lavoro e studi paghe.', caratteristiche: ['Più aziende clienti', 'Accesso per conto del cliente', 'Modelli di export dedicati', 'Assistenza prioritaria'], cta: 'Contattaci', url: '/contact' }
        ],
        nota: 'Prezzi IVA esclusa. Nessun vincolo: disdici quando vuoi.',
        domande: [
          { domanda: 'I dipendenti devono installare qualcosa?', risposta: 'No: timbrano e fanno le richieste dal browser dello smartphone o del computer.' },
          { domanda: 'Posso usarlo con il mio programma paghe?', risposta: 'Sì: l’export delle causali si configura con modelli adatti al programma paghe che usi.' },
          { domanda: 'Chi approva le richieste?', risposta: 'Gli approvatori del reparto del dipendente; l’amministratore può approvare per tutti.' },
          { domanda: 'Sono un consulente del lavoro: posso gestire più aziende?', risposta: 'Sì, con il piano Studio gestisci le aziende clienti ed entri per loro conto quando serve.' }
        ]
      },
      about: {
        titolo: 'Chi siamo',
        sottotitolo: 'Lavoriamo da anni accanto a consulenti del lavoro, uffici del personale e aziende.',
        paragrafi: [
          'Conosciamo da vicino il lavoro di chi chiude le presenze a fine mese: fogli da ricontrollare, richieste via email, causali da ricopiare nel programma paghe.',
          'SiSignore nasce per togliere questo lavoro: i dipendenti chiedono, i responsabili approvano e i dati arrivano alle paghe già controllati.'
        ],
        valori: [
          { icon: 'happy-outline', title: 'Semplice per i dipendenti', text: 'Timbrare e chiedere un permesso richiede pochi secondi.' },
          { icon: 'shield-checkmark-outline', title: 'Preciso per l’ufficio del personale', text: 'Regole orarie, causali e approvazioni sempre tracciate.' },
          { icon: 'chatbubbles-outline', title: 'Vicino ai consulenti', text: 'Export e modelli pensati con chi elabora le paghe.' }
        ]
      },
      register: {
        titolo: 'Registra la tua azienda',
        sottotitolo: 'Crea l’account della tua azienda: la gestione presenze è subito attiva, con causali e parametri pronti.',
        vantaggi: ['Gratis per 30 giorni', 'Gestione presenze già attiva', 'Nessuna carta di credito'],
        condizioni: 'Accetto le condizioni del servizio e l’informativa sulla privacy',
        esito: 'La tua azienda è pronta con la gestione presenze attiva. Questi sono gli utenti creati: accedi con la password che hai scelto.'
      }
    };
  }
}
