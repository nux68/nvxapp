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
});
