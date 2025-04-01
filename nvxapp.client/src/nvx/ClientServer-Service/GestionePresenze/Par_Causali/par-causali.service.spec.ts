import { TestBed } from '@angular/core/testing';

import { ParCausaliService } from './par-causali.service';

describe('ParCausaliService', () => {
  let service: ParCausaliService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParCausaliService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
