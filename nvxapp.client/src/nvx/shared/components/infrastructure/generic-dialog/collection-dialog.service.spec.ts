import { TestBed } from '@angular/core/testing';

import { CollectionDialogService } from './collection-dialog.service';

describe('CollectionDialogService', () => {
  let service: CollectionDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CollectionDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
