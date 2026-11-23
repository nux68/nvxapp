import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable } from '@angular/core';
import { HoverPopupComponent, HoverPopupData } from './hover-popup.component';

@Injectable({
  providedIn: 'root'
})
export class HoverPopupService {
  private componentRef: ComponentRef<HoverPopupComponent> | null = null;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector
  ) { }

  show(data: HoverPopupData, x: number, y: number): void {
    this.hide();

    this.componentRef = createComponent(HoverPopupComponent, {
      environmentInjector: this.injector
    });

    this.componentRef.instance.data = data;
    // primo disegno nascosto, per misurare le dimensioni reali del popup
    this.componentRef.instance.style = { top: '0px', left: '0px', visibility: 'hidden' };

    this.appRef.attachView(this.componentRef.hostView);
    document.body.appendChild(this.componentRef.location.nativeElement);
    // chiamato anche fuori da Angular (HoverPopupDirective): il popup si disegna subito
    this.componentRef.changeDetectorRef.detectChanges();

    this.componentRef.instance.style = this.calculatePosition(x, y);
    this.componentRef.changeDetectorRef.detectChanges();
  }

  updatePosition(x: number, y: number): void {
    if (this.componentRef) {
      this.componentRef.instance.style = this.calculatePosition(x, y);
      this.componentRef.changeDetectorRef.detectChanges();
    }
  }

  // accanto al cursore; se non ci sta a destra/sotto va a sinistra/sopra. Si usano le dimensioni reali del popup:
  // con quelle fisse (320x160) un popup corto vicino al bordo destro (bottoni delle liste) finiva lontano dal mouse
  private calculatePosition(x: number, y: number): Record<string, string> {
    const offset = 12;
    const margin = 4;
    const el = this.componentRef?.location.nativeElement.querySelector('.nvx-hover-popup') as HTMLElement | null;
    const rect = el?.getBoundingClientRect();
    const popupWidth = rect?.width || 320;
    const popupHeight = rect?.height || 160;
    const vw = window.innerWidth;
    const vh = window.innerHeight;

    let left = (x + offset + popupWidth > vw) ? x - popupWidth - offset : x + offset;
    let top  = (y + offset + popupHeight > vh) ? y - popupHeight - offset : y + offset;
    left = Math.max(margin, left);
    top = Math.max(margin, top);

    return {
      top: `${top}px`,
      left: `${left}px`
    };
  }

  hide(): void {
    if (this.componentRef) {
      this.appRef.detachView(this.componentRef.hostView);
      this.componentRef.destroy();
      this.componentRef = null;
    }
  }
}
