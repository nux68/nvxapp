import { TestBed } from '@angular/core/testing';

import { UsersUtilityService } from './users-utility.service';

describe('UsersUtilityService', () => {
  let service: UsersUtilityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UsersUtilityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
