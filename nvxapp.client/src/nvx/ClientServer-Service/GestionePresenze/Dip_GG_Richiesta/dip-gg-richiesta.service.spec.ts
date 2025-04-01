import { TestBed } from '@angular/core/testing';

import { DipGGRichiestaService } from './dip-gg-richiesta.service';

describe('DipGGRichiestaService', () => {
  let service: DipGGRichiestaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGRichiestaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
