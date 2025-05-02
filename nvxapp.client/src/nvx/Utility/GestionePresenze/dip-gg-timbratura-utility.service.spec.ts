import { TestBed } from '@angular/core/testing';

import { DipGGTimbraturaUtilityService } from './dip-gg-timbratura-utility.service';

describe('DipGGTimbraturaUtilityService', () => {
  let service: DipGGTimbraturaUtilityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DipGGTimbraturaUtilityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
