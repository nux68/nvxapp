import { TestBed } from '@angular/core/testing';
import { ParAttivitaService } from './par-attivita.service';

describe('AzAttivitaService', () => {
  let service: ParAttivitaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParAttivitaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
