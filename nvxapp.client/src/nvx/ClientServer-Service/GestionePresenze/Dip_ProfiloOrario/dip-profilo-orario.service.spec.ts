import { TestBed } from '@angular/core/testing';

import { DipProfiloOrarioService } from './dip-profilo-orario.service';

describe('DipProfiloOrarioService', () => {
  let service: DipProfiloOrarioService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipProfiloOrarioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
