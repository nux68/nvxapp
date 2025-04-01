import { TestBed } from '@angular/core/testing';

import { AzSediService } from './az-sedi.service';

describe('AzSediService', () => {
  let service: AzSediService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzSediService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
