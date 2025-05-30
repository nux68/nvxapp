import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AzCommessaService } from './az-commessa.service';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { Az_Commessa_GetAll_InModel, Az_Commessa_GetAll_OutModel } from './Models/az-commessa-model';

// Mock AuthService
class MockAuthService {}

describe('AzCommessaService', () => {
  let service: AzCommessaService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AzCommessaService, { provide: AuthService, useClass: MockAuthService }]
    });
    service = TestBed.inject(AzCommessaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call GetAll and return data', () => {
    const mockRequest: GenericRequest<Az_Commessa_GetAll_InModel> = { data: {} } as any;
    const mockResponse: any = { data: { az_Commessa: [] } };

    service.GetAll(mockRequest).subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(environment.remoteData.apiUri + 'Az_Commessa/GetAll');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });
});
