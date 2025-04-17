import { TestBed } from '@angular/core/testing';

import { FabMenuService } from './fab-menu.service';

describe('FabMenuService', () => {
  let service: FabMenuService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FabMenuService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
