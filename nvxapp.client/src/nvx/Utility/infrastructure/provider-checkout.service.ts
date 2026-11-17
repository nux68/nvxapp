import { Injectable, NgZone } from '@angular/core';
import { Observable } from 'rxjs';

/** esito che la pagina di ritorno (assets/pagamento-ritorno.html) passa all'app */
export interface EsitoProvider {
  esito: 'ok' | 'annullato';
  token: string;
}

/*
 Pagamento ospitato dal provider (PayPal, ANALISI_INFRASTRUCTURE_LICENZE.md §7.4) in una finestra a parte.
 Il login dell'app vive solo in memoria: portare la pagina su PayPal e ritorno la perderebbe. Quindi:
    - apri(): apre la finestra SUBITO, nello stesso clic dell'utente (dopo una chiamata al server il browser la bloccherebbe)
    - vai(): porta la finestra sulla pagina del provider quando il server ha restituito l'indirizzo; chiudi() se non serve
    - esiti(): esiti dalla pagina di ritorno, via BroadcastChannel (anche se il provider stacca window.opener) e postMessage
 La conferma la fa la pagina con la sessione dell'utente (LicensePaymentConfirm).
*/
@Injectable({ providedIn: 'root' })
export class ProviderCheckoutService {

  private static readonly Canale = 'nvx-pagamento';
  private finestra: Window | null = null;

  constructor(private zone: NgZone) { }

  apri(): Window | null {
    try {
      const w = window.open('', 'nvxPagamento', 'popup,width=520,height=760');
      if (w) {
        try {
          w.document.title = 'PayPal';
          w.document.body.style.cssText = 'font-family:system-ui,sans-serif;text-align:center;padding:32px';
          w.document.body.textContent = '…';
        } catch { }
      }
      this.finestra = w;
      return w;
    } catch {
      this.finestra = null;
      return null;
    }
  }

  /** false = finestra non disponibile (bloccata dal browser) */
  vai(url: string): boolean {
    if (!this.finestra || this.finestra.closed)
      return false;
    this.finestra.location.href = url;
    try { this.finestra.focus(); } catch { }
    return true;
  }

  chiudi() {
    try { this.finestra?.close(); } catch { }
    this.finestra = null;
  }

  esiti(): Observable<EsitoProvider> {
    return new Observable<EsitoProvider>(subscriber => {
      const visti = new Set<string>();
      const ricevi = (dati: any) => {
        if (!dati || dati.tipo !== ProviderCheckoutService.Canale || !dati.token || visti.has(dati.token))
          return;
        visti.add(dati.token);
        this.zone.run(() => subscriber.next({ esito: dati.esito === 'ok' ? 'ok' : 'annullato', token: String(dati.token) }));
      };

      let canale: BroadcastChannel | null = null;
      try {
        canale = new BroadcastChannel(ProviderCheckoutService.Canale);
        canale.onmessage = ev => ricevi(ev.data);
      } catch { }
      const messaggio = (ev: MessageEvent) => {
        if (ev.origin === window.location.origin)
          ricevi(ev.data);
      };
      window.addEventListener('message', messaggio);

      return () => {
        window.removeEventListener('message', messaggio);
        try { canale?.close(); } catch { }
      };
    });
  }
}
