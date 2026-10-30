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

  private showDelay = 500;
  private delayTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly removeListeners: (() => void)[] = [];

  constructor(private popupService: HoverPopupService,
              elementRef: ElementRef<HTMLElement>,
              zone: NgZone) {
    // eventi del mouse fuori da Angular: con @HostListener ogni movimento sopra un elemento con il popup
    // (decine per pagina, es. Calendario HR) aggiornava tutta l'app. Il popup si aggiorna da solo (HoverPopupService)
    zone.runOutsideAngular(() => {
      const el = elementRef.nativeElement;
      const on = (evento: string, handler: (e: MouseEvent) => void) => {
        el.addEventListener(evento, handler as EventListener);
        this.removeListeners.push(() => el.removeEventListener(evento, handler as EventListener));
      };
      on('mouseenter', e => this.onMouseEnter(e));
      on('mousemove', e => this.onMouseMove(e));
      on('mouseleave', () => this.onMouseLeave());
    });
  }

  private onMouseEnter(event: MouseEvent): void {
    if (!this.appHoverPopupEnabled) return;
    this.clearTimer();
    this.delayTimer = setTimeout(() => {
      this.popupService.show(this.popupData, event.clientX, event.clientY);
    }, this.showDelay);
  }

  private onMouseMove(event: MouseEvent): void {
    if (this.delayTimer) {
      this.popupService.updatePosition(event.clientX, event.clientY);
    }
  }

  private onMouseLeave(): void {
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
  }
}
