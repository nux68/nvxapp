import { Directive, ElementRef, Input, NgZone, OnDestroy } from '@angular/core';
import { HoverPopupData } from './hover-popup.component';
import { HoverPopupService } from './hover-popup.service';

@Directive({
  selector: '[appHoverPopup]',
  standalone: true
})
export class HoverPopupDirective implements OnDestroy {
  @Input('appHoverPopup') popupData!: HoverPopupData;
  @Input() appHoverPopupEnabled: boolean = true;
  /**
   * Su touch: 'tap' = il popup compare toccando (celle dei calendari); 'press' = solo tenendo premuto (pulsanti): il tocco
   * semplice esegue l'azione senza popup, la pressione lunga mostra il popup senza eseguire l'azione.
   */
  @Input() appHoverPopupTouch: 'tap' | 'press' = 'tap';

  private showDelay = 500;
  private delayTimer: ReturnType<typeof setTimeout> | null = null;
  private hideTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly removeListeners: (() => void)[] = [];

  // 'press': tocco in corso, popup mostrato dalla pressione, momento dell'ultimo tocco (dopo il tocco il browser
  // genera anche eventi del mouse, da ignorare)
  private inizioTocco: { x: number, y: number } | null = null;
  private pressioneMostrata = false;
  private ultimoTocco = 0;

  constructor(private popupService: HoverPopupService,
              private elementRef: ElementRef<HTMLElement>,
              zone: NgZone) {
    // eventi del mouse fuori da Angular: con @HostListener ogni movimento sopra un elemento con il popup
    // (decine per pagina, es. Calendario HR) aggiornava tutta l'app. Il popup si aggiorna da solo (HoverPopupService)
    zone.runOutsideAngular(() => {
      const el = elementRef.nativeElement;
      const on = (evento: string, handler: (e: any) => void, capture = false) => {
        el.addEventListener(evento, handler as EventListener, { capture, passive: evento.startsWith('touch') });
        this.removeListeners.push(() => el.removeEventListener(evento, handler as EventListener, { capture }));
      };
      on('mouseenter', e => this.onMouseEnter(e));
      on('mousemove', e => this.onMouseMove(e));
      on('mouseleave', () => this.onMouseLeave());
      on('touchstart', (e: TouchEvent) => this.onTouchStart(e));
      on('touchmove', (e: TouchEvent) => this.onTouchMove(e));
      on('touchend', () => this.onTouchEnd());
      on('touchcancel', () => this.onTouchEnd());
      on('contextmenu', (e: MouseEvent) => this.onContextMenu(e));
      on('click', (e: MouseEvent) => this.onClick(e), true);
    });
  }

  // pulsanti ('press'): centrato sopra il pulsante, altrimenti accanto al cursore
  private mostra(x: number, y: number): void {
    const sopra = this.appHoverPopupTouch === 'press' ? this.elementRef.nativeElement.getBoundingClientRect() : null;
    this.popupService.show(this.popupData, x, y, sopra);
  }

  private get daTocco(): boolean {
    return this.appHoverPopupTouch === 'press' && Date.now() - this.ultimoTocco < 1500;
  }

  private onTouchStart(event: TouchEvent): void {
    if (this.appHoverPopupTouch !== 'press') return;
    this.ultimoTocco = Date.now();
    this.pressioneMostrata = false;
    this.clearTimer();
    if (!this.appHoverPopupEnabled || event.touches.length !== 1) return;
    const t = event.touches[0];
    this.inizioTocco = { x: t.clientX, y: t.clientY };
    this.delayTimer = setTimeout(() => {
      this.pressioneMostrata = true;
      this.mostra(t.clientX, t.clientY);
    }, this.showDelay);
  }

  // il dito si sposta: non e' una pressione
  private onTouchMove(event: TouchEvent): void {
    const t = event.touches[0];
    if (!this.inizioTocco || !t) return;
    if (Math.abs(t.clientX - this.inizioTocco.x) > 10 || Math.abs(t.clientY - this.inizioTocco.y) > 10)
      this.onTouchEnd();
  }

  // il popup resta ancora un momento dopo aver alzato il dito
  private onTouchEnd(): void {
    if (this.appHoverPopupTouch !== 'press') return;
    this.ultimoTocco = Date.now();
    this.inizioTocco = null;
    this.clearTimer();
    if (this.pressioneMostrata)
      this.hideTimer = setTimeout(() => {
        this.pressioneMostrata = false;
        this.popupService.hide();
      }, 1200);
  }

  // pressione lunga: niente menu contestuale del browser
  private onContextMenu(event: MouseEvent): void {
    if (this.daTocco)
      event.preventDefault();
  }

  private onClick(event: MouseEvent): void {
    if (this.appHoverPopupTouch !== 'press') return;
    if (this.pressioneMostrata) {
      // click generato rilasciando la pressione lunga: era solo per leggere il popup
      event.preventDefault();
      event.stopImmediatePropagation();
      return;
    }
    // azione eseguita: il popup non deve comparire (o restare) sulla pagina che si apre
    this.clearTimer();
    this.popupService.hide();
  }

  private onMouseEnter(event: MouseEvent): void {
    if (!this.appHoverPopupEnabled || this.daTocco) return;
    this.clearTimer();
    this.delayTimer = setTimeout(() => {
      this.mostra(event.clientX, event.clientY);
    }, this.showDelay);
  }

  private onMouseMove(event: MouseEvent): void {
    if (this.delayTimer) {
      this.popupService.updatePosition(event.clientX, event.clientY);
    }
  }

  private onMouseLeave(): void {
    if (this.daTocco) return;
    this.clearTimer();
    this.popupService.hide();
  }

  ngOnDestroy(): void {
    this.removeListeners.forEach(remove => remove());
    this.clearTimer();
    this.popupService.hide();
  }

  private clearTimer(): void {
    if (this.delayTimer) {
      clearTimeout(this.delayTimer);
      this.delayTimer = null;
    }
    if (this.hideTimer) {
      clearTimeout(this.hideTimer);
      this.hideTimer = null;
    }
  }
}
