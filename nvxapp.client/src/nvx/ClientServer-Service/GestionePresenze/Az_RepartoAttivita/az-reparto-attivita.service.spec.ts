import { TestBed } from '@angular/core/testing';

import { AzRepartoAttivitaService } from './az-reparto-attivita.service';

describe('AzRepartoAttivitaService', () => {
  let service: AzRepartoAttivitaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzRepartoAttivitaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
