import { TestBed } from '@angular/core/testing';

import { TimeSheetUtilityService } from './time-sheet-utility.service';

describe('DipGGTimbraturaUtilityService', () => {
  let service: TimeSheetUtilityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeSheetUtilityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
