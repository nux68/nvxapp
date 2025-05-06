import { TestBed } from '@angular/core/testing';

import { AzSediRepartoService } from './az-sedi-reparto.service';

describe('AzRepartoService', () => {
  let service: AzSediRepartoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzSediRepartoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
