import { TestBed } from '@angular/core/testing';

import { RefresherService } from './refresher.service';

describe('RefresherService', () => {
  let service: RefresherService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RefresherService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
