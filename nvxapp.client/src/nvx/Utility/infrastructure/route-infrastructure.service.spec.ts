import { TestBed } from '@angular/core/testing';

import { RouteInfrastructureService } from './route-infrastructure.service';

describe('RouteInfrastructureService', () => {
  let service: RouteInfrastructureService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RouteInfrastructureService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
