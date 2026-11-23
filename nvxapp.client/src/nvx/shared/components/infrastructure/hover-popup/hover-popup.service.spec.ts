import { TestBed } from '@angular/core/testing';

import { HoverPopupService } from './hover-popup.service';

describe('HoverPopupService', () => {
  let service: HoverPopupService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HoverPopupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  afterEach(() => service.hide());

  it('vicino al bordo destro mette il popup subito a sinistra del cursore, non a 320px', () => {
    const x = window.innerWidth - 20;
    const y = 100;
    service.show({ content: 'Modifica' }, x, y);
    const rect = (document.querySelector('.nvx-hover-popup') as HTMLElement).getBoundingClientRect();
    expect(rect.right).toBeLessThanOrEqual(x);
    expect(x - rect.right).toBeLessThanOrEqual(20);
    expect(Math.abs(rect.top - (y + 12))).toBeLessThanOrEqual(1);
  });
});
