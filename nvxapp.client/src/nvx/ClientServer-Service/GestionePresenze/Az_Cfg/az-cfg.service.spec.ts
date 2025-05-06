import { TestBed } from '@angular/core/testing';

import { AzCfgService } from './az-cfg.service';

describe('AzCfgService', () => {
  let service: AzCfgService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzCfgService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
