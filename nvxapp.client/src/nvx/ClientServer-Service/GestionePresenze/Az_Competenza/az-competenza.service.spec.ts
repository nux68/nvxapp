import { TestBed } from '@angular/core/testing';
import { AzCompetenzaService } from './az-competenza.service';

describe('AzCompetenzaService', () => {
  let service: AzCompetenzaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AzCompetenzaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
