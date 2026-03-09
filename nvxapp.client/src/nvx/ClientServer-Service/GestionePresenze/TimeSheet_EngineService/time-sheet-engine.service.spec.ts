import { TestBed } from '@angular/core/testing';

import { TimeSheetEngineService } from './time-sheet-engine.service';

describe('TimeSheetEngineService', () => {
  let service: TimeSheetEngineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeSheetEngineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
