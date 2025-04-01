import { TestBed } from '@angular/core/testing';

import { ParArrotondamentiService } from './par-arrotondamenti.service';

describe('ParArrotondamentiService', () => {
  let service: ParArrotondamentiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParArrotondamentiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
