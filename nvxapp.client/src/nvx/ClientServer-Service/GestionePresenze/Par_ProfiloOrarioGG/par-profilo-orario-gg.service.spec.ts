import { TestBed } from '@angular/core/testing';

import { ParProfiloOrarioGGService } from './par-profilo-orario-gg.service';

describe('ParProfiloOrarioGGService', () => {
  let service: ParProfiloOrarioGGService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParProfiloOrarioGGService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
