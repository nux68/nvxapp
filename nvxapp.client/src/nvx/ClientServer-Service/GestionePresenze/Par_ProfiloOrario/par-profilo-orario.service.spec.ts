import { TestBed } from '@angular/core/testing';

import { ParProfiloOrarioService } from './par-profilo-orario.service';

describe('ParProfiloOrarioService', () => {
  let service: ParProfiloOrarioService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParProfiloOrarioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
