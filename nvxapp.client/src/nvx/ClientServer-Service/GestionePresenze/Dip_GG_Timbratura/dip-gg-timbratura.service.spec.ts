import { TestBed } from '@angular/core/testing';

import { DipGGTimbraturaService } from './dip-gg-timbratura.service';

describe('DipGGTimbraturaService', () => {
  let service: DipGGTimbraturaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGTimbraturaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
