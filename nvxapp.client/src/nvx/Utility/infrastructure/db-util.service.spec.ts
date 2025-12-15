import { TestBed } from '@angular/core/testing';

import { DbUtilService } from './db-util.service';

describe('DbUtilService', () => {
  let service: DbUtilService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DbUtilService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
