import { TestBed } from '@angular/core/testing';

import { DipGGGiustificativiService } from './dip-gg-giustificativi.service';

describe('DipGGGiustificativiService', () => {
  let service: DipGGGiustificativiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGGiustificativiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
