import { TestBed } from '@angular/core/testing';

import { AzRepartoService } from './az-reparto.service';

describe('AzRepartoService', () => {
  let service: AzRepartoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzRepartoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
