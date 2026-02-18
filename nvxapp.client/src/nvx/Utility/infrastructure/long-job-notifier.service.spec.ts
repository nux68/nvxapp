import { TestBed } from '@angular/core/testing';

import { LongJobNotifierService } from './long-job-notifier.service';

describe('LongJobNotifierService', () => {
  let service: LongJobNotifierService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LongJobNotifierService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
