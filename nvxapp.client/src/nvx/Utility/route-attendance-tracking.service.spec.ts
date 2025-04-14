import { TestBed } from '@angular/core/testing';

import { RouteAttendanceTrackingService } from './route-attendance-tracking.service';

describe('RouteAttendanceTrackingService', () => {
  let service: RouteAttendanceTrackingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RouteAttendanceTrackingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
