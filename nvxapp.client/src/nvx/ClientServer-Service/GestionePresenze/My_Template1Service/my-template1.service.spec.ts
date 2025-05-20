import { TestBed } from '@angular/core/testing';

import { MyTemplate1Service } from './my-template1.service';

describe('MyTemplate1ServiceService', () => {
  let service: MyTemplate1Service;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyTemplate1Service);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
