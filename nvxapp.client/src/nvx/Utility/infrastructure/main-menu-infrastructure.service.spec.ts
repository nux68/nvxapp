import { TestBed } from '@angular/core/testing';

import { MainMenuInfrastructureService } from './main-menu-infrastructure.service';

describe('MainMenuInfrastructureService', () => {
  let service: MainMenuInfrastructureService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MainMenuInfrastructureService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
