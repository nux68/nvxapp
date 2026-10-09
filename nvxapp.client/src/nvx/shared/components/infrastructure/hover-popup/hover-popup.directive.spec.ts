import { TestBed } from '@angular/core/testing';
import { HoverPopupDirective } from './hover-popup.directive';
import { HoverPopupService } from './hover-popup.service';

describe('HoverPopupDirective', () => {
  it('should create an instance', () => {
    const directive = new HoverPopupDirective(TestBed.inject(HoverPopupService));
    expect(directive).toBeTruthy();
  });
});
