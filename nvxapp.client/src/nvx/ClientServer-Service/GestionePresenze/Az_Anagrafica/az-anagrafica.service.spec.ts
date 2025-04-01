import { TestBed } from '@angular/core/testing';

import { AzAnagraficaService } from './az-anagrafica.service';

describe('AzAnagraficaService', () => {
  let service: AzAnagraficaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzAnagraficaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
