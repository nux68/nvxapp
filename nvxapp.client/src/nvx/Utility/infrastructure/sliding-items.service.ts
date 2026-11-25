import { Injectable, NgZone } from '@angular/core';

/** classe che Ionic mette sull'ion-item-sliding finche' e' aperto (o si sta trascinando) */
const APERTI = 'ion-item-sliding.item-sliding-active-slide';
/** oltre questo spostamento (px) il tocco e' un trascinamento, non un clic */
const TOLLERANZA_CLIC = 10;

/**
 * Righe a scorrimento (ion-item-sliding: nvx-list su mobile e righe delle pagine di dettaglio).
 * Ionic richiude la riga aperta solo quando se ne trascina un'altra. Qui, per tutta l'app, si richiude anche:
 *  - toccando un qualunque altro punto dell'interfaccia
 *  - toccando la riga stessa, fuori dai bottoni
 *  - scegliendo uno dei bottoni (tornando dal dettaglio la riga e' chiusa)
 * Basta crearlo una volta (AppComponent). Ascolta fuori dalla zona di Angular: nessun change detection in piu'.
 */
@Injectable({ providedIn: 'root' })
export class SlidingItemsService {

  /** riga gia' aperta toccata fuori dai bottoni: si chiude al clic, se non e' diventato un trascinamento */
  private toccata: { riga: HTMLIonItemSlidingElement, x: number, y: number } | null = null;

  constructor(ngZone: NgZone) {
    ngZone.runOutsideAngular(() => {
      // in cattura: anche se il componente toccato ferma l'evento
      document.addEventListener('pointerdown', e => this.premuto(e), { capture: true, passive: true });
      document.addEventListener('click', e => this.cliccato(e), { capture: true, passive: true });
    });
  }

  private premuto(e: PointerEvent): void {
    this.toccata = null;
    const bersaglio = e.target instanceof Element ? e.target : null;

    document.querySelectorAll<HTMLIonItemSlidingElement>(APERTI).forEach(riga => {
      if (!bersaglio || !riga.contains(bersaglio))
        this.chiudi(riga);
      else if (!bersaglio.closest('ion-item-options'))
        this.toccata = { riga, x: e.clientX, y: e.clientY };
    });
  }

  private cliccato(e: MouseEvent): void {
    const toccata = this.toccata;
    this.toccata = null;

    const bottone = e.target instanceof Element ? e.target.closest<HTMLIonItemOptionElement>('ion-item-option') : null;
    if (bottone) {
      const riga = bottone.closest<HTMLIonItemSlidingElement>('ion-item-sliding');
      if (riga && !bottone.disabled)
        this.chiudi(riga);
    }
    else if (toccata && Math.abs(e.clientX - toccata.x) <= TOLLERANZA_CLIC && Math.abs(e.clientY - toccata.y) <= TOLLERANZA_CLIC) {
      this.chiudi(toccata.riga);
    }
  }

  private chiudi(riga: HTMLIonItemSlidingElement): void {
    // close non c'e' finche' il componente di Ionic non e' pronto; la riga puo' sparire prima della chiusura
    riga.close?.().catch(() => { });
  }
}
