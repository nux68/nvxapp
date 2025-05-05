import { TestBed } from '@angular/core/testing';

import { AzRepartoUserServiceService } from './az-reparto-user-service.service';

describe('AzRepartoUserServiceService', () => {
  let service: AzRepartoUserServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzRepartoUserServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
