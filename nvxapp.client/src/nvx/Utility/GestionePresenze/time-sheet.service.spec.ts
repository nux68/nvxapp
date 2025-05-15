import { TestBed } from '@angular/core/testing';

import { TimeSheetService } from './time-sheet.service';

describe('MokeTimeSheetService', () => {
  let service: TimeSheetService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeSheetService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
