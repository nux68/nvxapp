import { Directive, Input, HostListener, OnDestroy } from '@angular/core';
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

  constructor(private popupService: HoverPopupService) { }

  @HostListener('mouseenter', ['$event'])
  onMouseEnter(event: MouseEvent): void {
    if (!this.appHoverPopupEnabled) return;
    this.clearTimer();
    this.delayTimer = setTimeout(() => {
      this.popupService.show(this.popupData, event.clientX, event.clientY);
    }, this.showDelay);
  }

  @HostListener('mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    if (this.delayTimer) {
      this.popupService.updatePosition(event.clientX, event.clientY);
    }
  }

  @HostListener('mouseleave')
  onMouseLeave(): void {
    this.clearTimer();
    this.popupService.hide();
  }

  ngOnDestroy(): void {
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
