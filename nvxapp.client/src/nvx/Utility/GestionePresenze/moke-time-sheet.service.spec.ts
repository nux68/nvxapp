import { TestBed } from '@angular/core/testing';

import { MokeTimeSheetService } from './moke-time-sheet.service';

describe('MokeTimeSheetService', () => {
  let service: MokeTimeSheetService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MokeTimeSheetService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
