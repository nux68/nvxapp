import { TestBed } from '@angular/core/testing';

import { MainMenuAttendanceTrackingService } from './main-menu-attendance-tracking.service';

describe('MainMenuAttendanceTrackingService', () => {
  let service: MainMenuAttendanceTrackingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MainMenuAttendanceTrackingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
