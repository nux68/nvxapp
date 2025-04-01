import { TestBed } from '@angular/core/testing';

import { ParOrarioService } from './par-orario.service';

describe('ParOrarioService', () => {
  let service: ParOrarioService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParOrarioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
