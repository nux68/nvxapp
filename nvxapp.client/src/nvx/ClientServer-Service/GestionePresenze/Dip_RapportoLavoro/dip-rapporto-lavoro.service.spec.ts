import { TestBed } from '@angular/core/testing';

import { DipRapportoLavoroService } from './dip-rapporto-lavoro.service';

describe('DipRapportoLavoroService', () => {
  let service: DipRapportoLavoroService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipRapportoLavoroService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
