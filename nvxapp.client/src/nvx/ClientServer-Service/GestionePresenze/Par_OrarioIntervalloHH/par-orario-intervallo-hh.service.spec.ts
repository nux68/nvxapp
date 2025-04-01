import { TestBed } from '@angular/core/testing';

import { ParOrarioIntervalloHHService } from './par-orario-intervallo-hh.service';

describe('ParOrarioIntervalloHHService', () => {
  let service: ParOrarioIntervalloHHService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParOrarioIntervalloHHService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
