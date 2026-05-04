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
    this.componentRef.instance.style = this.calculatePosition(x, y);

    this.appRef.attachView(this.componentRef.hostView);
    document.body.appendChild(this.componentRef.location.nativeElement);
  }

  updatePosition(x: number, y: number): void {
    if (this.componentRef) {
      this.componentRef.instance.style = this.calculatePosition(x, y);
      this.componentRef.changeDetectorRef.detectChanges();
    }
  }

  private calculatePosition(x: number, y: number): Record<string, string> {
    const offset = 12;
    const popupWidth = 320;
    const popupHeight = 160;
    const vw = window.innerWidth;
    const vh = window.innerHeight;

    const left = (x + offset + popupWidth > vw) ? x - popupWidth - offset : x + offset;
    const top  = (y + offset + popupHeight > vh) ? y - popupHeight - offset : y + offset;

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
