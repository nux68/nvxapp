import { TestBed } from '@angular/core/testing';

import { ChatAIService } from './chat-ai.service';

describe('ChatAIService', () => {
  let service: ChatAIService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ChatAIService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
