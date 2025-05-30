import { TestBed } from '@angular/core/testing';
import { AzAttivitaService } from './az-attivita.service';

describe('AzAttivitaService', () => {
  let service: AzAttivitaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzAttivitaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
