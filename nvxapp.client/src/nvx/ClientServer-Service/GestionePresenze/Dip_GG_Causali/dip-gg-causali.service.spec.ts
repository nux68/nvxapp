import { TestBed } from '@angular/core/testing';

import { DipGGCausaliService } from './dip-gg-causali.service';

describe('DipGGCausaliService', () => {
  let service: DipGGCausaliService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGCausaliService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
