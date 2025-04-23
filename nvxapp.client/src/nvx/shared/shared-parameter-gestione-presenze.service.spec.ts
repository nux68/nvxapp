import { TestBed } from '@angular/core/testing';

import { SharedParameterGestionePresenzeService } from './shared-parameter-gestione-presenze.service';

describe('SharedParameterGestionePresenzeService', () => {
  let service: SharedParameterGestionePresenzeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SharedParameterGestionePresenzeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
