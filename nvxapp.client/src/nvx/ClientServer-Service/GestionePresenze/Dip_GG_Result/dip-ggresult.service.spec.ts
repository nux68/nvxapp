import { TestBed } from '@angular/core/testing';

import { DipGGResultService } from './dip-ggresult.service';

describe('DipGGResultService', () => {
  let service: DipGGResultService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGResultService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
