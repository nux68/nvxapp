import { TestBed } from '@angular/core/testing';

import { MyMokeLongJobService } from './my-moke-long-job.service';

describe('MyMokeLongJobService', () => {
  let service: MyMokeLongJobService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyMokeLongJobService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
