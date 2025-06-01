import { TestBed } from '@angular/core/testing';
import { ParCompetenzaService } from './par-competenza.service';

describe('AzCompetenzaService', () => {
  let service: ParCompetenzaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParCompetenzaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
