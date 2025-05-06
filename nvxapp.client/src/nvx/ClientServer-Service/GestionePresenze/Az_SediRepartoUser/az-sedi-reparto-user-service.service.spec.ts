import { TestBed } from '@angular/core/testing';

import { AzSediRepartoUserServiceService } from './az-sedi-reparto-user-service.service';

describe('AzRepartoUserServiceService', () => {
  let service: AzSediRepartoUserServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzSediRepartoUserServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
