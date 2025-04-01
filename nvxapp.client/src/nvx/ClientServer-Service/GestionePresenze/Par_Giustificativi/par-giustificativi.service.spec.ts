import { TestBed } from '@angular/core/testing';

import { ParGiustificativiService } from './par-giustificativi.service';

describe('ParGiustificativiService', () => {
  let service: ParGiustificativiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParGiustificativiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
