import { TestBed } from '@angular/core/testing';

import { MonthNavigatorService } from './month-navigator.service';

describe('MonthNavigatorService', () => {
  let service: MonthNavigatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MonthNavigatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
