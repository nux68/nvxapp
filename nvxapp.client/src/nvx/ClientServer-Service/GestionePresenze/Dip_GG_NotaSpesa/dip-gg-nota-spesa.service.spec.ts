import { TestBed } from '@angular/core/testing';

import { DipGGNotaSpesaService } from './dip-gg-nota-spesa.service';

describe('DipGGNotaSpesaService', () => {
  let service: DipGGNotaSpesaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGNotaSpesaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
