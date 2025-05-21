import { TestBed } from '@angular/core/testing';

import { DateTimeUtilService } from './date-time-util.service';

describe('DateTimeUtilService', () => {
  let service: DateTimeUtilService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DateTimeUtilService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
