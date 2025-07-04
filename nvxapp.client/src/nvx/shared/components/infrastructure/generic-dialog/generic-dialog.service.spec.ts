import { TestBed } from '@angular/core/testing';

import { GenericDialogService } from './generic-dialog.service';

describe('GenericDialogService', () => {
  let service: GenericDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GenericDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
