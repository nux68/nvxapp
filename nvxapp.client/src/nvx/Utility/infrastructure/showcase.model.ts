/*
 Contenuti delle pagine dello showcase (sito pubblico, prima del login).
 Le pagine sono template: mostrano questi testi. Infrastructure fornisce testi generici
 (ShowcaseInfrastructureService); ogni ramo applicativo puo' ridefinire intere sezioni
 (es. ShowcaseAttendanceTrackingService per la gestione presenze), vedi ShowcaseService.
*/

/** Voce generica: icona, titolo e testo (punti di forza, passi, funzionalita', valori). */
export interface ShowcaseVoce {
  icon: string;
  title: string;
  text: string;
}

export interface ShowcasePiano {
  nome: string;
  prezzo: string;
  periodo: string;
  descrizione: string;
  caratteristiche: string[];
  cta: string;
  /** pagina del pulsante (predefinita: registrazione) */
  url?: string;
  /** piano consigliato, messo in evidenza */
  evidenza?: boolean;
}

export interface ShowcaseDomanda {
  domanda: string;
  risposta: string;
}

export interface ShowcaseContenuti {
  brand: {
    nome: string;
    icon: string;
    slogan: string;
  };
  home: {
    etichetta: string;
    titolo: string;
    sottotitolo: string;
    ctaPrimaria: string;
    ctaSecondaria: string;
    punti: ShowcaseVoce[];
    passiTitolo: string;
    passi: ShowcaseVoce[];
    chiusuraTitolo: string;
    chiusuraTesto: string;
  };
  features: {
    titolo: string;
    sottotitolo: string;
    sezioni: ShowcaseVoce[];
  };
  pricing: {
    titolo: string;
    sottotitolo: string;
    piani: ShowcasePiano[];
    nota: string;
    domande: ShowcaseDomanda[];
  };
  about: {
    titolo: string;
    sottotitolo: string;
    paragrafi: string[];
    valori: ShowcaseVoce[];
  };
  contact: {
    titolo: string;
    sottotitolo: string;
    email: string;
    telefono: string;
    indirizzo: string;
    orari: string;
    testoModulo: string;
  };
  register: {
    titolo: string;
    sottotitolo: string;
    vantaggi: string[];
    condizioni: string;
    /** dopo la richiesta: invito ad aprire il link ricevuto per email */
    inviata: string;
    /** pagina di conferma (/confirm-email): titolo e testo con gli utenti creati */
    confermaTitolo: string;
    esito: string;
  };
  /** recupero dell'accesso con password dimenticata (/forgot-password e /reset-password) */
  passwordRecovery: {
    titolo: string;
    sottotitolo: string;
    inviata: string;
    resetTitolo: string;
    resetSottotitolo: string;
    resetFatto: string;
  };
}

/** Ridefinizione di un ramo applicativo: solo le sezioni da sostituire (ognuna per intero). */
export type ShowcaseContenutiRidefiniti = Partial<ShowcaseContenuti>;
