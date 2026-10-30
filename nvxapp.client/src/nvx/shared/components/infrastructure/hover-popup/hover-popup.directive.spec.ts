import { ElementRef, NgZone } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HoverPopupDirective } from './hover-popup.directive';
import { HoverPopupService } from './hover-popup.service';

describe('HoverPopupDirective', () => {
  it('should create an instance', () => {
    const directive = new HoverPopupDirective(TestBed.inject(HoverPopupService), new ElementRef(document.createElement('div')), TestBed.inject(NgZone));
    expect(directive).toBeTruthy();
  });
});
