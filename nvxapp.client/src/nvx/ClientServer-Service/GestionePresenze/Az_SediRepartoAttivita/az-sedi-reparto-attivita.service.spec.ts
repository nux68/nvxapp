import { TestBed } from '@angular/core/testing';

import { AzSediRepartoAttivitaService } from './az-sedi-reparto-attivita.service';

describe('AzRepartoAttivitaService', () => {
  let service: AzSediRepartoAttivitaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzSediRepartoAttivitaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
