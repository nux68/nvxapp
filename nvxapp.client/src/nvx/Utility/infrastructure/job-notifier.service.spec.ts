import { TestBed } from '@angular/core/testing';

import { JobNotifierService } from './job-notifier.service';

describe('JobNotifierService', () => {
  let service: JobNotifierService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(JobNotifierService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
