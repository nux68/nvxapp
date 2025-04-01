import { TestBed } from '@angular/core/testing';

import { DipAnagraficaService } from './dip-anagrafica.service';

describe('DipAnagraficaService', () => {
  let service: DipAnagraficaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipAnagraficaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
